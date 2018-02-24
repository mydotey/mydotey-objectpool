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
        public virtual int QueueCapacity { get; private set; }

        protected internal ThreadPoolConfig()
        {

        }

        protected internal new class Builder : ObjectPoolConfig<WorkerThread>.Builder, IBuilder
        {
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

            public new virtual IThreadPoolConfig Build()
            {
                if (Config.QueueCapacity < 0)
                    throw new ArgumentException("queueCapacity is less than 0");

                return (IThreadPoolConfig)base.Build();
            }
        }
    }
}