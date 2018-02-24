using System;
using MyDotey.ObjectPool.ThreadPool;
using MyDotey.ObjectPool.ThreadPool.AutoScale;

/**
 * @author koqizhao
 *
 * Feb 23, 2018
 */
namespace MyDotey.ObjectPool.Facade
{
    public class ThreadPools
    {
        public static ThreadPool.IBuilder NewThreadPoolConfigBuilder()
        {
            return new ThreadPoolConfig.Builder();
        }

        public static IThreadPool NewThreadPool(IThreadPoolConfig config)
        {
            return new DefaultThreadPool(config);
        }

        public static ThreadPool.AutoScale.IBuilder NewAutoScaleThreadPoolConfigBuilder()
        {
            return new AutoScaleThreadPoolConfig.Builder();
        }

        public static IAutoScaleThreadPool NewAutoScaleThreadPool(IAutoScaleThreadPoolConfig config)
        {
            return new AutoScaleThreadPool(config);
        }
    }
}