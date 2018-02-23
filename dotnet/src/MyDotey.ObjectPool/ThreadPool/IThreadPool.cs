using System;

/**
 * @author koqizhao
 *
 * Feb 22, 2018
 */
namespace MyDotey.ObjectPool.ThreadPool
{
    public interface IThreadPool : IDisposable
    {
        int Size { get; }

        void Submit(Action task);

        bool TrySubmit(Action task);
    }
}