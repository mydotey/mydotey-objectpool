using System;
using MyDotey.ObjectPool.AutoScale;

/**
 * @author koqizhao
 *
 * Feb 23, 2018
 */
namespace MyDotey.ObjectPool.Facade
{
    public class ObjectPools
    {
        public static IBuilder<T> NewObjectPoolConfigBuilder<T>()
        {
            return new ObjectPoolConfig<T>.Builder();
        }

        public static IObjectPool<T> NewObjectPool<T>(IObjectPoolConfig<T> config)
        {
            return new ObjectPool<T>(config);
        }

        public static MyDotey.ObjectPool.AutoScale.IBuilder<T> NewAutoScaleObjectPoolConfigBuilder<T>()
        {
            return new AutoScaleObjectPoolConfig<T>.Builder();
        }

        public static IAutoScaleObjectPool<T> NewAutoScaleObjectPool<T>(IAutoScaleObjectPoolConfig<T> config)
        {
            return new AutoScaleObjectPool<T>(config);
        }
    }
}