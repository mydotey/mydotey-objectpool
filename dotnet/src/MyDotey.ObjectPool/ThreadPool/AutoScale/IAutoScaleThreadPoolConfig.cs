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

    public interface IBuilder : MyDotey.ObjectPool.ThreadPool.IBuilder
    {
        new IBuilder SetMinSize(int minSize);

        new IBuilder SetMaxSize(int maxSize);

        IBuilder SetMaxIdleTime(long maxIdleTime);

        IBuilder SetScaleFactor(int scaleFactor);

        IBuilder SetCheckInterval(long checkInterval);

        new IAutoScaleThreadPoolConfig Build();
    }
}