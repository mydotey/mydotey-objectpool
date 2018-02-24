using System;
using System.Collections.Concurrent;
using System.Threading;
using MyDotey.ObjectPool.ThreadPool;
using MyDotey.ObjectPool.Facade;

/**
 * @author koqizhao}
 *
 * Feb 23, 2018
 */
namespace MyDotey.ObjectPool.AutoScale
{
    public class AutoScaleObjectPool<T> : ObjectPool<T>, IAutoScaleObjectPool<T>
    {
        //private static Logger _logger = LoggerFactory.getLogger(AutoScaleObjectPool.class);

        protected internal static long CurrentTimeMillis { get { return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond; } }

        protected Timer _taskScheduler;

        protected volatile int _scalingOut;
        protected Action _scaleOutTask;

        protected IThreadPool _threadPool;

        public new virtual IAutoScaleObjectPoolConfig<T> Config { get { return (AutoScaleObjectPoolConfig<T>)base.Config; } }

        public AutoScaleObjectPool(IAutoScaleObjectPoolConfig<T> config)
            : base(config)
        {

        }

        protected override void Init()
        {
            _keyGenerator = new ObjectPool<T>.Entry.KeyGenerator();
            _availableKeys = new BlockingCollection<object>(new ConcurrentStack<object>());
            _entries = new ConcurrentDictionary<object, IEntry<T>>();

            TryAddNewEntry(Config.MinSize);

            _taskScheduler = new Timer(o => AutoCheck(), this, Config.CheckInterval, Config.CheckInterval);

            _scaleOutTask = () =>
            {
                try
                {
                    TryAddNewEntry(Config.ScaleFactor - 1);
                    //_logger.info("scaleOut success");
                }
                catch (Exception ex)
                {
                    //_logger.error("scaleOut failed", ex);
                }
                finally
                {
                    Interlocked.CompareExchange(ref _scalingOut, 1, 0);
                }
            };

            ThreadPool.IBuilder builder = ThreadPools.NewThreadPoolConfigBuilder();
            builder.SetMinSize(1).SetMaxSize(1).SetQueueSize(Config.MaxSize);
            _threadPool = ThreadPools.NewThreadPool(builder);
        }

        protected override ObjectPool<T>.Entry TryAddNewEntryAndAcquireOne()
        {
            ObjectPool<T>.Entry entry = TryCreateNewEntry();
            if (entry == null)
                return null;

            if (Interlocked.CompareExchange(ref _scalingOut, 0, 1) == 0)
                SubmitTaskSafe(_scaleOutTask);

            return base.DoAcquire(entry);
        }

        protected override void AddNewEntry(ObjectPool<T>.Entry entry)
        {
            lock (entry.Key)
            {
                base.AddNewEntry(entry);
            }
        }

        protected new virtual AutoScaleEntry NewPoolEntry(Object key)
        {
            return (AutoScaleEntry)base.NewPoolEntry(key);
        }

        protected override ObjectPool<T>.Entry NewConcretePoolEntry(Object key, T obj)
        {
            return new AutoScaleEntry(key, obj);
        }

        protected new virtual AutoScaleEntry GetEntry(Object key)
        {
            return (AutoScaleEntry)base.GetEntry(key);
        }

        protected override ObjectPool<T>.Entry TryAcquire(Object key)
        {
            ObjectPool<T>.Entry entry = DoAcquire(key);
            if (entry != null)
                return entry;

            return (ObjectPool<T>.Entry)TryAcquire();
        }

        protected override ObjectPool<T>.Entry Acquire(Object key)
        {
            ObjectPool<T>.Entry entry = DoAcquire(key);
            if (entry != null)
                return entry;

            return (ObjectPool<T>.Entry)Acquire();
        }

        protected override ObjectPool<T>.Entry DoAcquire(Object key)
        {
            lock (key)
            {
                AutoScaleEntry entry = GetEntry(key);
                if (entry == null)
                    return null;

                base.DoAcquire(entry);

                if (!NeedRefresh(entry))
                {
                    entry.Renew();
                    return entry;
                }
                else
                {
                    entry.Status = AutoScaleEntry.EntryStatus.PENDING_REFRESH;
                }
            }

            ReleaseKey(key);
            return null;
        }

        protected override void ReleaseKey(Object key)
        {
            SubmitTaskSafe(() => DoReleaseKey(key));
        }

        protected virtual void DoReleaseKey(Object key)
        {
            lock (key)
            {
                AutoScaleEntry entry = GetEntry(key);
                if (entry.Status == AutoScaleEntry.EntryStatus.PENDING_REFRESH)
                {
                    if (!TryRefresh(entry))
                    {
                        ScaleIn(entry);
                        return;
                    }
                }
                else
                    entry.Renew();

                base.ReleaseKey(key);
            }
        }

        protected virtual void AutoCheck()
        {
            foreach (Object key in _entries.Keys)
            {
                if (TryScaleIn(key))
                    continue;

                TryRefresh(key);
            }
        }

        protected virtual bool TryScaleIn(Object key)
        {
            AutoScaleEntry entry = GetEntry(key);
            if (!NeedScaleIn(entry))
                return false;

            lock (key)
            {
                entry = GetEntry(key);
                if (!NeedScaleIn(entry))
                    return false;

                ScaleIn(entry);
                return true;
            }
        }

        protected virtual void ScaleIn(AutoScaleEntry entry)
        {
            lock (AddLock)
            {
                _entries.TryRemove(entry.Key, out IEntry<T> value);
                Dispose(entry);
                //_logger.info("scaled in an object: {}", entry.Object);
            }
        }

        protected virtual bool TryRefresh(Object key)
        {
            AutoScaleEntry entry = GetEntry(key);
            if (!NeedRefresh(entry))
                return false;

            lock (key)
            {
                entry = GetEntry(key);
                if (!NeedRefresh(entry))
                    return false;

                if (entry.Status == AutoScaleEntry.EntryStatus.AVAILABLE)
                    return TryRefresh(entry);

                entry.Status = AutoScaleEntry.EntryStatus.PENDING_REFRESH;
                return false;
            }
        }

        protected virtual bool TryRefresh(AutoScaleEntry entry)
        {
            AutoScaleEntry newEntry = null;
            try
            {
                newEntry = NewPoolEntry(entry.Key);
            }
            catch (Exception e)
            {
                String errorMessage = String.Format("failed to refresh object: {0}, still use it", entry.Object);
                //_logger.error(errorMessage, e);
                return false;
            }

            Dispose(entry);
            _entries.TryAdd(entry.Key, newEntry);

            //_logger.info("refreshed an object, old: {0}, new: {1}", entry.Object, newEntry.Object);
            return true;
        }

        protected virtual bool NeedRefresh(AutoScaleEntry entry)
        {
            return IsExpired(entry) || IsStale(entry);
        }

        protected virtual bool IsExpired(AutoScaleEntry entry)
        {
            return entry.CreationTime <= CurrentTimeMillis - Config.ObjectTtl;
        }

        protected virtual bool IsStale(AutoScaleEntry entry)
        {
            try
            {
                return Config.StaleChecker(entry.Object);
            }
            catch (Exception e)
            {
                //_logger.error("staleChecker failed, ignore", e);
                return false;
            }
        }

        protected virtual bool NeedScaleIn(AutoScaleEntry entry)
        {
            return entry.Status == AutoScaleEntry.EntryStatus.AVAILABLE
                    && entry.LastUsedTime <= CurrentTimeMillis - Config.MaxIdleTime
                    && Size > Config.MinSize;
        }

        protected override void DoDispose()
        {
            base.DoDispose();

            try
            {
                _taskScheduler.Dispose();
                _threadPool.Dispose();
            }
            catch (Exception e)
            {
                // _logger.error("shutdown timer failed.", e);
            }
        }

        protected virtual void SubmitTaskSafe(Action task)
        {
            try
            {
                _threadPool.Submit(task);
            }
            catch (Exception ex)
            {
                task();
            }
        }

        protected internal class AutoScaleEntry : ObjectPool<T>.Entry
        {
            public new class EntryStatus : ObjectPool<T>.Entry.EntryStatus
            {
                public const String PENDING_REFRESH = "pending_refresh";
            }

            public virtual long CreationTime { get; }
            public virtual long LastUsedTime { get; set; }

            public AutoScaleEntry(Object key, T obj)
                : base(key, obj)
            {
                CreationTime = CurrentTimeMillis;
                LastUsedTime = CreationTime;
            }

            public virtual void Renew()
            {
                LastUsedTime = CurrentTimeMillis;
            }
        }
    }
}