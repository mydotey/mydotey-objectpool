using System;
using MyDotey.ObjectPool.AutoScale;
using MyDotey.ObjectPool.Facade;

/**
 * @author koqizhao
 *
 * Feb 22, 2018
 */
namespace MyDotey.ObjectPool.ThreadPool.AutoScale
{
    public class AutoScaleThreadPool : DefaultThreadPool
    {
        public AutoScaleThreadPool(IBuilder builder)
            : base(builder)
        {
        }

        protected override IObjectPool<WorkerThread> NewObjectPool(ThreadPool.IBuilder builder)
        {
            AutoScaleObjectPoolConfig<WorkerThread> config =
                (AutoScaleObjectPoolConfig<WorkerThread>)((AutoScaleThreadPoolConfig.Builder)builder).SetThreadPool(this).Build();
            return ObjectPools.NewAutoScaleObjectPool(config);
        }
    }
}