using System;

/**
 * @author koqizhao
 *
 * Feb 22, 2018
 */
namespace MyDotey.ObjectPool
{
    public interface IObjectPoolEntry<T>
    {
        T Object { get; }
    }
}
