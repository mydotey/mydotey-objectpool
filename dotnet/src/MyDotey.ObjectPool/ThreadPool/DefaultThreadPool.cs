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
            ObjectPool = NewObjectPool(builder);
        }

        protected virtual IObjectPool<WorkerThread> NewObjectPool(IBuilder builder)
        {
            ObjectPoolConfig<WorkerThread> config = (ObjectPoolConfig<WorkerThread>)((ThreadPoolConfig.Builder)builder).SetThreadPool(this).Build();
            return ObjectPools.NewObjectPool(config);
        }

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