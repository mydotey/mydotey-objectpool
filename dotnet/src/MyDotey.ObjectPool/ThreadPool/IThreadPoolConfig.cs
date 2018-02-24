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

        int QueueSize { get; }
    }

    public interface IBuilder
    {
        IBuilder SetMinSize(int minSize);

        IBuilder SetMaxSize(int maxSize);

        IBuilder SetQueueSize(int queueSize);

        IThreadPoolConfig Build();
    }
}