using System;

/**
 * @author koqizhao
 *
 * Feb 22, 2018
 */
namespace MyDotey.ObjectPool.ThreadPool.AutoScale
{
    public interface IAutoScaleThreadPoolConfig : IThreadPoolConfig
    {
        long MaxIdleTime { get; }

        int ScaleFactor { get; }

        long CheckInterval { get; }
    }

    public interface IBuilder : IAbstractBuilder<IBuilder>
    {

    }

    public interface IAbstractBuilder<B> : ThreadPool.IAbstractBuilder<B>
        where B : IAbstractBuilder<B>
    {
        B SetMaxIdleTime(long maxIdleTime);

        B SetScaleFactor(int scaleFactor);

        B SetCheckInterval(long checkInterval);

        new IAutoScaleThreadPoolConfig Build();
    }
}