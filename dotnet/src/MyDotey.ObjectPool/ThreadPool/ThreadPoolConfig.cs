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
        protected internal ThreadPoolConfig()
        {

        }

        protected internal new class Builder : ObjectPoolConfig<WorkerThread>.Builder, IBuilder
        {
            private DefaultThreadPool _threadPool;

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

            protected internal virtual IBuilder SetThreadPool(DefaultThreadPool pool)
            {
                _threadPool = pool;
                return this;
            }

            public new virtual IThreadPoolConfig Build()
            {
                if (_threadPool == null)
                    throw new ArgumentNullException("threadPool is null");

                DefaultThreadPool pool = _threadPool;
                SetObjectFactory(() => new WorkerThread(t => pool.ObjectPool.Release(t.PoolEntry)))
                        .SetOnCreate(e =>
                        {
                            e.Object.PoolEntry = e;
                            e.Object.Start();
                        })
                        .SetOnClose(e => e.Object.Interrupt());
                return (IThreadPoolConfig)base.Build();
            }
        }
    }
}