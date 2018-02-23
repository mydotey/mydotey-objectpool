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

        Action<IEntry<T>> OnCreate { get; }

        Action<IEntry<T>> OnClose { get; }
    }

    public interface IBuilder<T> : IAbstractBuilder<T, IBuilder<T>>
    {

    }

    public interface IAbstractBuilder<T, B>
        where B : IAbstractBuilder<T, B>
    {
        B SetMinSize(int minSize);

        B SetMaxSize(int maxSize);

        B SetObjectFactory(Func<T> objectFactory);

        B SetOnCreate(Action<IEntry<T>> onCreate);

        B SetOnClose(Action<IEntry<T>> onClose);

        IObjectPoolConfig<T> Build();
    }
}