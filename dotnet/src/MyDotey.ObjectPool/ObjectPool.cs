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

        protected ObjectPoolEntry.KeyGenerator _keyGenerator;
        protected BlockingCollection<Object> _availableKeys;
        protected ConcurrentDictionary<Object, IObjectPoolEntry<T>> _entries;

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
            _keyGenerator = new ObjectPoolEntry.KeyGenerator();
            _availableKeys = new BlockingCollection<object>(new ConcurrentStack<object>(), Config.MaxSize);
            _entries = new ConcurrentDictionary<object, IObjectPoolEntry<T>>(4, Config.MaxSize);

            TryAddNewEntry(Config.MinSize);
        }

        protected virtual void TryAddNewEntry(int count)
        {
            for (int i = 0; i < count; i++)
                TryAddNewEntry();
        }

        protected virtual ObjectPoolEntry TryAddNewEntry()
        {
            ObjectPoolEntry entry = TryCreateNewEntry();
            if (entry != null)
                AddNewEntry(entry);

            return entry;
        }

        protected virtual void AddNewEntry(ObjectPoolEntry entry)
        {
            _availableKeys.Add(entry.Key);
        }

        protected virtual ObjectPoolEntry TryCreateNewEntry()
        {
            if (IsDisposed)
                return null;

            lock (AddLock)
            {
                if (IsDisposed)
                    return null;

                if (Size == Config.MaxSize)
                    return null;

                ObjectPoolEntry entry = NewPoolEntry();
                entry.Status = ObjectPoolEntry.EntryStatus.AVAILABLE;
                _entries.TryAdd(entry.Key, entry);
                return entry;
            }
        }

        protected virtual ObjectPoolEntry NewPoolEntry()
        {
            return NewPoolEntry(_keyGenerator.GenerateKey());
        }

        protected virtual ObjectPoolEntry NewPoolEntry(object key)
        {
            ObjectPoolEntry entry = NewConcretePoolEntry(key, NewObject());
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

        protected virtual ObjectPoolEntry NewConcretePoolEntry(object key, T obj)
        {
            return new ObjectPoolEntry(key, obj);
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

        protected ObjectPoolEntry GetEntry(object key)
        {
            _entries.TryGetValue(key, out IObjectPoolEntry<T> value);
            return (ObjectPoolEntry)value;
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

        public virtual IObjectPoolEntry<T> Acquire()
        {
            CheckDisposed();

            IObjectPoolEntry<T> entry = TryAcquire();
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

        public virtual IObjectPoolEntry<T> TryAcquire()
        {
            if (IsDisposed)
                return null;

            _availableKeys.TryTake(out object key);
            if (key != null)
                return TryAcquire(key);

            return TryAddNewEntryAndAcquireOne();
        }

        protected virtual ObjectPoolEntry TryAcquire(object key)
        {
            return DoAcquire(key);
        }

        protected virtual ObjectPoolEntry Acquire(object key)
        {
            return DoAcquire(key);
        }

        protected virtual ObjectPoolEntry DoAcquire(object key)
        {
            ObjectPoolEntry entry = GetEntry(key);
            return DoAcquire(entry);
        }

        protected virtual ObjectPoolEntry TryAddNewEntryAndAcquireOne()
        {
            ObjectPoolEntry entry = TryCreateNewEntry();
            if (entry == null)
                return null;

            return DoAcquire(entry);
        }

        protected virtual ObjectPoolEntry DoAcquire(ObjectPoolEntry entry)
        {
            entry.Status = ObjectPoolEntry.EntryStatus.ACQUIRED;
            Interlocked.Increment(ref _acquiredSize);
            return (ObjectPoolEntry)entry.Clone();
        }

        public virtual void Release(IObjectPoolEntry<T> entry)
        {
            if (IsDisposed)
                return;

            ObjectPoolEntry defaultEntry = (ObjectPoolEntry)entry;
            if (defaultEntry == null || defaultEntry.Status == ObjectPoolEntry.EntryStatus.RELEASED)
                return;

            lock (defaultEntry)
            {
                if (defaultEntry.Status == ObjectPoolEntry.EntryStatus.RELEASED)
                    return;

                defaultEntry.Status = ObjectPoolEntry.EntryStatus.RELEASED;
                Interlocked.Decrement(ref _acquiredSize);
            }

            ReleaseKey(defaultEntry.Key);
        }

        protected virtual void ReleaseKey(object key)
        {
            GetEntry(key).Status = ObjectPoolEntry.EntryStatus.AVAILABLE;
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
            foreach (IObjectPoolEntry<T> entry in _entries.Values)
            {
                Dispose((ObjectPoolEntry)entry);
            }

            _availableKeys.Dispose();
        }

        protected void Dispose(ObjectPoolEntry entry)
        {
            entry.Status = ObjectPoolEntry.EntryStatus.CLOSED;

            try
            {
                Config.OnClose(entry);
            }
            catch (Exception e)
            {
                //_logger.error("Dispose object failed", e);
            }
        }

        protected internal class ObjectPoolEntry : IObjectPoolEntry<T>, ICloneable
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

            public ObjectPoolEntry(object key, T obj)
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