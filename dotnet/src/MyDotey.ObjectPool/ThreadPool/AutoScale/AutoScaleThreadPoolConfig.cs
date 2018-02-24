using System;
using System.Threading;
using MyDotey.ObjectPool.AutoScale;

/**
 * @author koqizhao
 *
 * Feb 22, 2018
 */
namespace MyDotey.ObjectPool.ThreadPool.AutoScale
{
    public class AutoScaleThreadPoolConfig : AutoScaleObjectPoolConfig<WorkerThread>, IAutoScaleThreadPoolConfig
    {
        public virtual int QueueCapacity { get; protected set; }

        protected internal AutoScaleThreadPoolConfig()
        {

        }

        protected internal new class Builder : AutoScaleObjectPoolConfig<WorkerThread>.Builder, IBuilder
        {
            private IThreadPool _threadPool;

            public Builder()
            {
                SetQueueSize(int.MaxValue);
            }

            protected override ObjectPoolConfig<WorkerThread> NewPoolConfig()
            {
                return new AutoScaleThreadPoolConfig();
            }

            protected new virtual AutoScaleThreadPoolConfig Config { get { return (AutoScaleThreadPoolConfig)base.Config; } }

            ThreadPool.IBuilder ThreadPool.IBuilder.SetMinSize(int minSize)
            {
                return (ThreadPool.IBuilder)base.SetMinSize(minSize);
            }

            public new virtual IBuilder SetMinSize(int minSize)
            {
                return (IBuilder)base.SetMinSize(minSize);
            }

            ThreadPool.IBuilder ThreadPool.IBuilder.SetMaxSize(int maxSize)
            {
                return (ThreadPool.IBuilder)base.SetMaxSize(maxSize);
            }

            public new virtual IBuilder SetMaxSize(int maxSize)
            {
                return (IBuilder)base.SetMaxSize(maxSize);
            }

            ThreadPool.IBuilder ThreadPool.IBuilder.SetQueueCapacity(int queueCapacity)
            {
                return SetQueueSize(queueCapacity);
            }

            public virtual IBuilder SetQueueSize(int queueCapacity)
            {
                Config.QueueCapacity = queueCapacity;
                return this;
            }

            public new virtual IBuilder SetMaxIdleTime(long maxIdleTime)
            {
                return (IBuilder)base.SetMaxIdleTime(maxIdleTime);
            }

            public new virtual IBuilder SetScaleFactor(int scaleFactor)
            {
                return (IBuilder)base.SetScaleFactor(scaleFactor);
            }

            public new virtual IBuilder SetCheckInterval(long checkInterval)
            {
                return (IBuilder)base.SetCheckInterval(checkInterval);
            }

            protected internal virtual IBuilder SetThreadPool(IThreadPool pool)
            {
                _threadPool = pool;
                return this;
            }

            ThreadPool.IThreadPoolConfig ThreadPool.IBuilder.Build()
            {
                return Build();
            }

            public new virtual IAutoScaleThreadPoolConfig Build()
            {
                if (Config.QueueCapacity < 0)
                    throw new ArgumentException("queueCapacity is less than 0");

                if (_threadPool == null)
                    throw new ArgumentNullException("threadPool is null");

                AutoScaleThreadPool pool = (AutoScaleThreadPool)_threadPool;
                SetObjectFactory(() => new WorkerThread(t => pool.ObjectPool.Release(t.PoolEntry)))
                    .SetOnCreate(e =>
                    {
                        e.Object.PoolEntry = e;
                        e.Object.Start();
                    })
                    .SetOnClose(e => e.Object.InnerThread.Interrupt())
                    .SetStaleChecker(t => t.InnerThread.ThreadState == ThreadState.Aborted
                        || t.InnerThread.ThreadState == ThreadState.Stopped);
                return (AutoScaleThreadPoolConfig)base.Build();
            }
        }
    }
}