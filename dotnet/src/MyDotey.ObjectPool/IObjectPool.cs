using System;

/**
 * @author koqizhao
 *
 * Feb 22, 2018
 */
namespace MyDotey.ObjectPool
{
    public interface IObjectPool<T> : IDisposable
    {
        IObjectPoolConfig<T> Config { get; }

        int Size { get; }

        int AcquiredSize { get; }

        int AvailableSize { get; }

        bool IsDisposed { get; }

        IEntry<T> Acquire();

        IEntry<T> TryAcquire();

        void Release(IEntry<T> entry);
    }

    public interface IEntry<T>
    {
        T Object { get; }
    }
}
