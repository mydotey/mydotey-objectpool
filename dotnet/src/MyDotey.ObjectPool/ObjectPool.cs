using System;
using System.Threading;
using System.Collections.Concurrent;
using NLog;

/**
 * @author koqizhao
 *
 * Feb 23, 2018
 */
namespace MyDotey.ObjectPool
{
    public class ObjectPool<T> : IObjectPool<T>
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger();

        public virtual IObjectPoolConfig<T> Config { get; }

        protected object AddLock { get; }

        public virtual bool IsClosed { get; protected set; }

        protected Func<Object> _keyGenerator;
        protected BlockingCollection<Object> _availableKeys;
        protected ConcurrentDictionary<Object, IEntry<T>> _entries;

        protected volatile int _acquiredSize;

        public ObjectPool(IObjectPoolConfig<T> config)
        {
            if (config == null)
                throw new ArgumentNullException("config is null");

            Config = config;

            AddLock = new object();

            _keyGenerator = NewKeyGenerator();
            _availableKeys = new BlockingCollection<object>(new ConcurrentStack<object>());
            _entries = new ConcurrentDictionary<object, IEntry<T>>();

            Init();
        }

        protected virtual Func<Object> NewKeyGenerator()
        {
            KeyGenerator keyGenerator = new KeyGenerator();
            return () => keyGenerator.GenerateKey();
        }

        protected virtual void Init()
        {
            TryAddNewEntry(Config.MinSize);
        }

        protected virtual void TryAddNewEntry(int count)
        {
            for (int i = 0; i < count; i++)
                TryAddNewEntry();
        }

        protected virtual Entry TryAddNewEntry()
        {
            Entry entry = TryCreateNewEntry();
            if (entry != null)
                AddNewEntry(entry);

            return entry;
        }

        protected virtual void AddNewEntry(Entry entry)
        {
            _availableKeys.Add(entry.Key);
        }

        protected virtual Entry TryCreateNewEntry()
        {
            if (IsClosed)
                return null;

            lock (AddLock)
            {
                if (IsClosed)
                    return null;

                if (Size == Config.MaxSize)
                    return null;

                Entry entry = NewPoolEntry();
                entry.Status = Entry.EntryStatus.Available;
                _entries.TryAdd(entry.Key, entry);
                return entry;
            }
        }

        protected virtual Entry NewPoolEntry()
        {
            return NewPoolEntry(_keyGenerator());
        }

        protected virtual Entry NewPoolEntry(object key)
        {
            Entry entry = NewConcretePoolEntry(key, NewObject());
            try
            {
                Config.OnCreate(entry);
            }
            catch (Exception e)
            {
                _logger.Error(e, "onEntryCreate failed");
            }

            return entry;
        }

        protected virtual Entry NewConcretePoolEntry(object key, T obj)
        {
            return new Entry(key, obj);
        }

        protected virtual T NewObject()
        {
            T obj = Config.ObjectFactory();
            if (obj == null)
            {
                _logger.Error("object factory generated null, the object factory has bug");
                throw new InvalidOperationException("object factory Generated null");
            }

            _logger.Info("new object created: {0}", obj);
            return obj;
        }

        protected virtual Entry GetEntry(object key)
        {
            _entries.TryGetValue(key, out IEntry<T> value);
            return (Entry)value;
        }

        public virtual int Size { get { return _entries.Count; } }

        public virtual int AcquiredSize { get { return _acquiredSize; } }

        public virtual int AvailableSize
        {
            get
            {
                int availableSize = Size - AcquiredSize;
                return availableSize > 0 ? availableSize : 0;
            }
        }

        public virtual IEntry<T> Acquire()
        {
            CheckClosed();

            IEntry<T> entry = TryAcquire();
            if (entry != null)
                return entry;

            object key = TakeFirst();
            return Acquire(key);
        }

        protected virtual object TakeFirst()
        {
            while (true)
            {
                CheckClosed();

                _availableKeys.TryTake(out object key, 1 * 1000);
                if (key != null)
                    return key;
            }
        }

        protected virtual void CheckClosed()
        {
            if (IsClosed)
                throw new ObjectDisposedException("object pool has been closed");
        }

        public virtual IEntry<T> TryAcquire()
        {
            if (IsClosed)
                return null;

            _availableKeys.TryTake(out object key);
            if (key != null)
                return TryAcquire(key);

            return TryAddNewEntryAndAcquireOne();
        }

        protected virtual Entry TryAcquire(object key)
        {
            return DoAcquire(key);
        }

        protected virtual Entry Acquire(object key)
        {
            return DoAcquire(key);
        }

        protected virtual Entry DoAcquire(object key)
        {
            Entry entry = GetEntry(key);
            return DoAcquire(entry);
        }

        protected virtual Entry TryAddNewEntryAndAcquireOne()
        {
            Entry entry = TryCreateNewEntry();
            if (entry == null)
                return null;

            return DoAcquire(entry);
        }

        protected virtual Entry DoAcquire(Entry entry)
        {
            entry.Status = Entry.EntryStatus.Acquired;
            Interlocked.Increment(ref _acquiredSize);
            return (Entry)entry.Clone();
        }

        public virtual void Release(IEntry<T> entry)
        {
            if (IsClosed)
                return;

            Entry defaultEntry = (Entry)entry;
            if (defaultEntry == null || defaultEntry.Status == Entry.EntryStatus.Released)
                return;

            lock (defaultEntry)
            {
                if (defaultEntry.Status == Entry.EntryStatus.Released)
                    return;

                defaultEntry.Status = Entry.EntryStatus.Released;
                Interlocked.Decrement(ref _acquiredSize);
            }

            ReleaseKey(defaultEntry.Key);
        }

        protected virtual void ReleaseKey(object key)
        {
            GetEntry(key).Status = Entry.EntryStatus.Available;
            _availableKeys.Add(key);
        }

        public virtual void Dispose()
        {
            if (IsClosed)
                return;

            lock (AddLock)
            {
                if (IsClosed)
                    return;

                IsClosed = true;
                DoClose();
            }
        }

        protected virtual void DoClose()
        {
            foreach (IEntry<T> entry in _entries.Values)
            {
                Close((Entry)entry);
            }

            _availableKeys.Dispose();
        }

        protected void Close(Entry entry)
        {
            entry.Status = Entry.EntryStatus.Closed;

            try
            {
                Config.OnClose(entry);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Close object failed");
            }
        }

        protected internal class Entry : IEntry<T>, ICloneable
        {
            public class EntryStatus
            {
                public const string Available = "available";
                public const string Acquired = "acquired";
                public const string Released = "released";
                public const string Closed = "closed";
            }

            public virtual object Key { get; }

            public virtual string Status { get; set; }

            public virtual T Object { get; }

            public Entry(object key, T obj)
            {
                Key = key;
                Object = obj;
            }

            public virtual object Clone()
            {
                return MemberwiseClone();
            }
        }

        protected internal class KeyGenerator
        {
            private long _counter;
            private const long MAX = long.MaxValue / 2;

            public KeyGenerator()
            {

            }

            public object GenerateKey()
            {
                long count = Interlocked.Increment(ref _counter);
                if (count > MAX)
                    _logger.Warn("{0} objects created, maybe misused", count);

                return count;
            }
        }
    }
}