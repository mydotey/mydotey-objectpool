using System;

/**
 * @author koqizhao
 *
 * Feb 23, 2018
 */
namespace MyDotey.ObjectPool.AutoScale
{
    public interface IAutoScaleObjectPoolConfig<T> : IObjectPoolConfig<T>
    {
        long ObjectTtl { get; }

        long MaxIdleTime { get; }

        Func<T, bool> StaleChecker { get; }

        long CheckInterval { get; }

        int ScaleFactor { get; }
    }

    public interface IBuilder<T> : IAbstractBuilder<T, IBuilder<T>>
    {

    }

    public interface IAbstractBuilder<T, B> : MyDotey.ObjectPool.IAbstractBuilder<T, B>
        where B : IAbstractBuilder<T, B>
    {
        B SetObjectTtl(long objectTtl);

        B SetMaxIdleTime(long maxIdleTime);

        B SetStaleChecker(Func<T, bool> staleChecker);

        B SetCheckInterval(long checkInterval);

        B SetScaleFactor(int scaleFactor);

        new IAutoScaleObjectPoolConfig<T> Build();
    }
}