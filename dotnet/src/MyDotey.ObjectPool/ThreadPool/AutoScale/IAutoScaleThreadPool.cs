using System;

/**
 * @author koqizhao
 *
 * Feb 22, 2018
 */
namespace MyDotey.ObjectPool.ThreadPool.AutoScale
{
    public interface IAutoScaleThreadPool : IThreadPool
    {
        new IThreadPoolConfig Config { get; }
    }
}