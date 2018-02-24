using System;
using System.Threading;
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
        public AutoScaleThreadPool(IAutoScaleThreadPoolConfig config)
            : base(config)
        {
        }

        public new virtual IAutoScaleThreadPoolConfig Config { get { return (IAutoScaleThreadPoolConfig)base.Config; } }

        protected override IObjectPool<WorkerThread> NewObjectPool()
        {
            ObjectPool.AutoScale.IBuilder<WorkerThread> builder = ObjectPools.NewAutoScaleObjectPoolConfigBuilder<WorkerThread>();
            builder.SetCheckInterval(Config.CheckInterval).SetMaxIdleTime(Config.MaxIdleTime)
                    .SetScaleFactor(Config.ScaleFactor)
                    .SetStaleChecker(t => t.InnerThread.ThreadState == ThreadState.Aborted
                        || t.InnerThread.ThreadState == ThreadState.Stopped);
            SetObjectPoolConfigBuilder<ObjectPool.AutoScale.IBuilder<WorkerThread>>(builder);
            return ObjectPools.NewAutoScaleObjectPool(builder.Build());
        }
    }
}