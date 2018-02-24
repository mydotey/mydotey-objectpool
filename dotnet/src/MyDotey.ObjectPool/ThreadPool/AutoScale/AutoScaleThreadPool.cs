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
    public class AutoScaleThreadPool : DefaultThreadPool, IAutoScaleThreadPool
    {
        public AutoScaleThreadPool(IBuilder builder)
            : base(builder)
        {
        }

        public new virtual IAutoScaleThreadPoolConfig Config { get { return (IAutoScaleThreadPoolConfig)base.Config; } }

        protected override IThreadPoolConfig NewConfig(ThreadPool.IBuilder builder)
        {
            return ((AutoScaleThreadPoolConfig.Builder)builder).SetThreadPool(this).Build();
        }

        protected override IObjectPool<WorkerThread> NewObjectPool()
        {
            return ObjectPools.NewAutoScaleObjectPool((AutoScaleObjectPoolConfig<WorkerThread>)Config);
        }
    }
}