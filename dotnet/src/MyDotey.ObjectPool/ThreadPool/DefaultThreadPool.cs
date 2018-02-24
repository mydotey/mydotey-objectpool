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
        public virtual IThreadPoolConfig Config { get; }
        protected internal virtual IObjectPool<WorkerThread> ObjectPool { get; }

        protected virtual bool HasQueue { get; }
        protected virtual BlockingCollection<Action> TaskQueue { get; }
        protected virtual Thread TaskConsumer { get; }

        public DefaultThreadPool(IThreadPoolConfig config)
        {
            if (config == null)
                throw new ArgumentNullException("config is null");

            Config = config;
            ObjectPool = NewObjectPool();

            HasQueue = Config.QueueCapacity > 0;
            if (HasQueue)
            {
                TaskQueue = new BlockingCollection<Action>(new ConcurrentQueue<Action>(), Config.QueueCapacity);
                TaskConsumer = new Thread(ConsumeTask)
                {
                    IsBackground = true
                };
                TaskConsumer.Start();
            }
        }

        protected virtual IObjectPool<WorkerThread> NewObjectPool()
        {
            IBuilder<WorkerThread> builder = ObjectPools.NewObjectPoolConfigBuilder<WorkerThread>();
            SetObjectPoolConfigBuilder<IBuilder<WorkerThread>>(builder);
            return ObjectPools.NewObjectPool(builder.Build());
        }

        protected virtual void SetObjectPoolConfigBuilder<B>(IAbstractBuilder<WorkerThread, B> builder)
            where B : IAbstractBuilder<WorkerThread, B>
        {
            builder.SetMaxSize(Config.MaxSize)
                    .SetMinSize(Config.MinSize)
                    .SetObjectFactory(() => new WorkerThread(t => ObjectPool.Release(t.PoolEntry)))
                    .SetOnCreate(e =>
                    {
                        e.Object.PoolEntry = e;
                        e.Object.Start();
                    })
                    .SetOnClose(e => e.Object.InnerThread.Interrupt());
        }

        public virtual int Size { get { return ObjectPool.Size; } }

        public virtual int QueueSize { get { return HasQueue ? TaskQueue.Count : 0; } }

        public virtual void Submit(Action task)
        {
            if (task == null)
                throw new ArgumentNullException("task is null");

            if (HasQueue)
            {
                if (TrySubmit(task))
                    return;

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