using System;

/**
 * @author koqizhao
 *
 * Feb 22, 2018
 */
namespace MyDotey.ObjectPool.ThreadPool
{
    public class ThreadPoolConfig : ObjectPoolConfig<WorkerThread>, IThreadPoolConfig
    {
        public virtual int QueueCapacity { get; protected set; }

        protected internal ThreadPoolConfig()
        {

        }

        protected internal new class Builder : ObjectPoolConfig<WorkerThread>.Builder, IBuilder
        {
            private IThreadPool _threadPool;

            public Builder()
            {
                SetQueueCapacity(int.MaxValue);
            }

            protected override ObjectPoolConfig<WorkerThread> NewPoolConfig()
            {
                return new ThreadPoolConfig();
            }

            protected new virtual ThreadPoolConfig Config { get { return (ThreadPoolConfig)base.Config; } }

            public new virtual IBuilder SetMinSize(int minSize)
            {
                return (IBuilder)base.SetMinSize(minSize);
            }

            public new virtual IBuilder SetMaxSize(int maxSize)
            {
                return (IBuilder)base.SetMaxSize(maxSize);
            }

            public virtual IBuilder SetQueueCapacity(int queueCapacity)
            {
                Config.QueueCapacity = queueCapacity;
                return this;
            }

            protected internal virtual IBuilder SetThreadPool(IThreadPool pool)
            {
                _threadPool = pool;
                return this;
            }

            public new virtual IThreadPoolConfig Build()
            {
                if (Config.QueueCapacity < 0)
                    throw new ArgumentException("queueCapacity is less than 0");

                if (_threadPool == null)
                    throw new ArgumentNullException("threadPool is null");

                DefaultThreadPool pool = (DefaultThreadPool)_threadPool;
                SetObjectFactory(() => new WorkerThread(t => pool.ObjectPool.Release(t.PoolEntry)))
                    .SetOnCreate(e =>
                    {
                        e.Object.PoolEntry = e;
                        e.Object.Start();
                    })
                    .SetOnClose(e => e.Object.InnerThread.Interrupt());
                return (IThreadPoolConfig)base.Build();
            }
        }
    }
}