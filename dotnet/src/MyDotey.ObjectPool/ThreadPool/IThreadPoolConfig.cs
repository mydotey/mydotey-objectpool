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

    public interface IBuilder : IAbstractBuilder<IBuilder>
    {

    }

    public interface IAbstractBuilder<B>
        where B : IAbstractBuilder<B>
    {
        B SetMinSize(int minSize);

        B SetMaxSize(int maxSize);

        B SetQueueCapacity(int queueCapacity);

        IThreadPoolConfig Build();
    }
}