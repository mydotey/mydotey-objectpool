using System;

/**
 * @author koqizhao
 *
 * Feb 23, 2018
 */
namespace MyDotey.ObjectPool
{

    public class ObjectPools
    {

        public static IObjectPoolConfigBuilder<T> newObjectPoolConfigBuilder<T>()
        {
            return new ObjectPoolConfig<T>.Builder();
        }

        public static IObjectPool<T> newObjectPool<T>(IObjectPoolConfig<T> config)
        {
            return new ObjectPool<T>(config);
        }

    }

}