using System;

/**
 * @author koqizhao
 *
 * Feb 22, 2018
 */
namespace MyDotey.ObjectPool
{
    public interface IObjectPoolConfig<T>
    {
        int MinSize { get; }

        int MaxSize { get; }

        Func<T> ObjectFactory { get; }

        Action<IObjectPoolEntry<T>> OnCreate { get; }

        Action<IObjectPoolEntry<T>> OnClose { get; }
    }
}