using System;
using System.Collections.Concurrent;
using System.Threading;
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
        public virtual IThreadPoolConfig Config { get; protected set; }
        protected internal virtual IObjectPool<WorkerThread> ObjectPool { get; }

        protected virtual bool HasQueue { get; }
        protected virtual BlockingCollection<Action> TaskQueue { get; }
        protected virtual Thread TaskConsumer { get; }

        public DefaultThreadPool(IBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder is null");

            Config = NewConfig(builder);
            ObjectPool = NewObjectPool();

            HasQueue = Config.QueueSize > 0;
            if (HasQueue)
            {
                TaskQueue = new BlockingCollection<Action>(new ConcurrentQueue<Action>(), Config.QueueSize);
                TaskConsumer = new Thread(ConsumeTask)
                {
                    IsBackground = true
                };
                TaskConsumer.Start();
            }
        }

        protected virtual IThreadPoolConfig NewConfig(IBuilder builder)
        {
            return ((ThreadPoolConfig.Builder)builder).SetThreadPool(this).Build();
        }

        protected virtual IObjectPool<WorkerThread> NewObjectPool()
        {
            return ObjectPools.NewObjectPool((ObjectPoolConfig<WorkerThread>)Config);
        }

        public virtual int Size { get { return ObjectPool.Size; } }

        public virtual int QueueSize { get { return HasQueue ? TaskQueue.Count : 0; } }

        public virtual void Submit(Action task)
        {
            if (task == null)
                throw new ArgumentNullException("task is null");

            if (HasQueue)
            {
                TaskQueue.Add(task);
                return;
            }

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

        protected virtual void ConsumeTask()
        {
            while (true)
            {
                try
                {
                    Action task = TaskQueue.Take();
                    IEntry<WorkerThread> entry = ObjectPool.Acquire();
                    entry.Object.SetTask(task);
                }
                catch (Exception ex)
                {
                    //
                    break;
                }
            }
        }

        public virtual void Dispose()
        {
            ObjectPool.Dispose();

            if (HasQueue)
                TaskConsumer.Interrupt();
        }
    }
}