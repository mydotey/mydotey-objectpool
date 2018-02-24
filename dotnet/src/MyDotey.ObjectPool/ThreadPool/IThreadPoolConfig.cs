using System;

/**
 * @author koqizhao
 *
 * Feb 22, 2018
 */
namespace MyDotey.ObjectPool.ThreadPool
{
    public interface IThreadPoolConfig
    {
        int MinSize { get; }

        int MaxSize { get; }

        int QueueCapacity { get; }
    }

    public interface IBuilder
    {
        IBuilder SetMinSize(int minSize);

        IBuilder SetMaxSize(int maxSize);

        IBuilder SetQueueCapacity(int queueCapacity);

        IThreadPoolConfig Build();
    }
}