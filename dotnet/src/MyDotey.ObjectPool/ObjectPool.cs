using System;
using System.Threading;
using System.Collections.Concurrent;

/**
 * @author koqizhao
 *
 * Feb 23, 2018
 */
namespace MyDotey.ObjectPool
{
    public class ObjectPool<T> : IObjectPool<T>
    {
        //private static Logger _logger = LoggerFactory.GetLogger(ObjectPool.class);

        public virtual IObjectPoolConfig<T> Config { get; }

        protected object AddLock { get; }
        protected volatile bool _isDisposed;

        protected Entry.KeyGenerator _keyGenerator;
        protected BlockingCollection<Object> _availableKeys;
        protected ConcurrentDictionary<Object, IEntry<T>> _entries;

        protected volatile int _acquiredSize;

        public ObjectPool(IObjectPoolConfig<T> config)
        {
            if (config == null)
                throw new ArgumentNullException("config is null");

            Config = config;

            AddLock = new object();

            Init();
        }

        protected virtual void Init()
        {
            _keyGenerator = new Entry.KeyGenerator();
            _availableKeys = new BlockingCollection<object>(new ConcurrentStack<object>(), Config.MaxSize);
            _entries = new ConcurrentDictionary<object, IEntry<T>>(4, Config.MaxSize);

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
            if (IsDisposed)
                return null;

            lock (AddLock)
            {
                if (IsDisposed)
                    return null;

                if (Size == Config.MaxSize)
                    return null;

                Entry entry = NewPoolEntry();
                entry.Status = Entry.EntryStatus.AVAILABLE;
                _entries.TryAdd(entry.Key, entry);
                return entry;
            }
        }

        protected virtual Entry NewPoolEntry()
        {
            return NewPoolEntry(_keyGenerator.GenerateKey());
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
                //_logger.error("onEntryCreate failed", e);
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
                //_logger.error("object factory Generated null, the object factory has bug");
                throw new InvalidOperationException("object factory Generated null");
            }

            //_logger.info("new object created: {}", obj);
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

        public virtual bool IsDisposed { get { return _isDisposed; } }

        public virtual IEntry<T> Acquire()
        {
            CheckDisposed();

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
                CheckDisposed();

                _availableKeys.TryTake(out object key, 1 * 1000);
                if (key != null)
                    return key;
            }
        }

        protected virtual void CheckDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException("object pool has been Disposed");
        }

        public virtual IEntry<T> TryAcquire()
        {
            if (IsDisposed)
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
            entry.Status = Entry.EntryStatus.ACQUIRED;
            Interlocked.Increment(ref _acquiredSize);
            return (Entry)entry.Clone();
        }

        public virtual void Release(IEntry<T> entry)
        {
            if (IsDisposed)
                return;

            Entry defaultEntry = (Entry)entry;
            if (defaultEntry == null || defaultEntry.Status == Entry.EntryStatus.RELEASED)
                return;

            lock (defaultEntry)
            {
                if (defaultEntry.Status == Entry.EntryStatus.RELEASED)
                    return;

                defaultEntry.Status = Entry.EntryStatus.RELEASED;
                Interlocked.Decrement(ref _acquiredSize);
            }

            ReleaseKey(defaultEntry.Key);
        }

        protected virtual void ReleaseKey(object key)
        {
            GetEntry(key).Status = Entry.EntryStatus.AVAILABLE;
            _availableKeys.Add(key);
        }

        public virtual void Dispose()
        {
            if (IsDisposed)
                return;

            lock (AddLock)
            {
                if (IsDisposed)
                    return;

                _isDisposed = true;
                DoDispose();
            }
        }

        protected virtual void DoDispose()
        {
            foreach (IEntry<T> entry in _entries.Values)
            {
                Dispose((Entry)entry);
            }

            _availableKeys.Dispose();
        }

        protected void Dispose(Entry entry)
        {
            entry.Status = Entry.EntryStatus.CLOSED;

            try
            {
                Config.OnClose(entry);
            }
            catch (Exception e)
            {
                //_logger.error("Dispose object failed", e);
            }
        }

        protected internal class Entry : IEntry<T>, ICloneable
        {
            public class EntryStatus
            {
                public const string AVAILABLE = "available";
                public const string ACQUIRED = "Acquired";
                public const string RELEASED = "Released";
                public const string CLOSED = "Disposed";
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

            public class KeyGenerator
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
                        ;//_logger.warn("{} objects created, maybe misused", count);

                    return count;
                }
            }
        }
    }
}