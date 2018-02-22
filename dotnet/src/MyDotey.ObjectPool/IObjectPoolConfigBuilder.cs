using System;

/**
 * @author koqizhao
 *
 * Feb 22, 2018
 */
namespace MyDotey.ObjectPool
{

    public interface IObjectPoolConfigBuilder<T> : IAbstractObjectPoolConfigBuilder<T, IObjectPoolConfigBuilder<T>>
    {

    }

    public interface IAbstractObjectPoolConfigBuilder<T, B>
        where B : IAbstractObjectPoolConfigBuilder<T, B>
    {
        B SetMinSize(int minSize);

        B SetMaxSize(int maxSize);

        B SetObjectFactory(Func<T> objectFactory);

        B SetOnCreate(Action<IObjectPoolEntry<T>> onCreate);

        B SetOnClose(Action<IObjectPoolEntry<T>> onClose);

        IObjectPoolConfig<T> Build();
    }
}