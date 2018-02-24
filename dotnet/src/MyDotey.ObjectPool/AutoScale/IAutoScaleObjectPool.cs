using System;

/**
 * @author koqizhao
 *
 * Feb 23, 2018
 */
namespace MyDotey.ObjectPool.AutoScale
{
    public interface IAutoScaleObjectPool<T> : IObjectPool<T>
    {
        new IAutoScaleObjectPoolConfig<T> Config { get; }
    }
}