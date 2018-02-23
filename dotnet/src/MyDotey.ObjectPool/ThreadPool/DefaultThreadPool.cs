using System;
using MyDotey.ObjectPool.Facade;

/**
 * @author koqizhao
 *
 * Feb 22, 2018
 */
namespace MyDotey.ObjectPool.ThreadPool
{
    public class DefaultThreadPool : IThreadPool
    {
        protected internal virtual IObjectPool<WorkerThread> ObjectPool { get; }

        public DefaultThreadPool(IBuilder builder)
        {
            ObjectPool = ObjectPools.NewObjectPool(NewObjectPoolConfig(builder));
        }

        protected virtual ObjectPoolConfig<WorkerThread> NewObjectPoolConfig(IBuilder builder)
        {
            return (ObjectPoolConfig<WorkerThread>)((ThreadPoolConfig.Builder)builder).SetThreadPool(this).Build();
        }

        /*
            public DefaultThreadPool(AutoScaleThreadPoolConfig.Builder builder)
            {
                _threadPool = ObjectPools.newAutoScaleObjectPool(newAutoScaleObjectPoolConfig(builder));
            }

            protected AutoScaleObjectPoolConfig<WorkerThread> newAutoScaleObjectPoolConfig(
                    AutoScaleThreadPoolConfig.Builder builder)
            {
                return ((DefaultAutoScaleThreadPoolConfig.Builder)builder).setThreadPool(this).build();
            }
         */

        public virtual int Size { get { return ObjectPool.Size; } }

        public virtual void Submit(Action task)
        {
            if (task == null)
                throw new ArgumentNullException("task is null");

            IEntry<WorkerThread> entry = ObjectPool.Acquire();
            entry.Object.SetTask(task);
        }

        public virtual bool TrySubmit(Action task)
        {
            if (task == null)
                throw new ArgumentNullException("task is null");

            IEntry<WorkerThread> entry = ObjectPool.TryAcquire();
            if (entry == null)
                return false;

            entry.Object.SetTask(task);
            return true;
        }

        public virtual void Dispose()
        {
            ObjectPool.Dispose();
        }
    }
}