using System;

/**
 * @author koqizhao
 *
 * Feb 22, 2018
 */
namespace MyDotey.ObjectPool
{
    public class ObjectPoolConfig<T> : IObjectPoolConfig<T>, ICloneable
    {
        public virtual int MinSize { get; private set; }

        public virtual int MaxSize { get; private set; }

        public virtual Func<T> ObjectFactory { get; private set; }

        public virtual Action<IEntry<T>> OnCreate { get; private set; }

        public virtual Action<IEntry<T>> OnClose { get; private set; }

        protected ObjectPoolConfig()
        {

        }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        protected internal class Builder : AbstractBuilder<IBuilder<T>>, IBuilder<T>
        {

        }

        protected internal abstract class AbstractBuilder<B> : IAbstractBuilder<T, B>
            where B : IAbstractBuilder<T, B>
        {

            //private static Logger _logger = LoggerFactory.getLogger\(objectPool.class);

            public static readonly Action<IEntry<T>> DefaultOnCreate = e => { };

            public static readonly Action<IEntry<T>> DefaultOnClose = e =>
            {
                if (e.Object is IDisposable)
                {
                    try
                    {
                        ((IDisposable)e.Object).Dispose();
                    }
                    catch (Exception ex)
                    {
                        //_logger.error("close object failed", ex);
                    }
                }
            };

            protected virtual ObjectPoolConfig<T> Config { get; }

            protected AbstractBuilder()
            {
                Config = NewPoolConfig();
                Config.OnCreate = DefaultOnCreate;
                Config.OnClose = DefaultOnClose;
            }

            protected virtual ObjectPoolConfig<T> NewPoolConfig()
            {
                return new ObjectPoolConfig<T>();
            }

            public virtual B SetMinSize(int minSize)
            {
                Config.MinSize = minSize;
                return (B)(object)this;
            }

            public virtual B SetMaxSize(int maxSize)
            {
                Config.MaxSize = maxSize;
                return (B)(object)this;
            }

            public virtual B SetObjectFactory(Func<T> objectFactory)
            {
                Config.ObjectFactory = objectFactory;
                return (B)(object)this;
            }

            public virtual B SetOnCreate(Action<IEntry<T>> onCreate)
            {
                Config.OnCreate = onCreate;
                return (B)(object)this;
            }

            public virtual B SetOnClose(Action<IEntry<T>> onClose)
            {
                Config.OnClose = onClose;
                return (B)(object)this;
            }

            public virtual IObjectPoolConfig<T> Build()
            {
                if (Config.MinSize < 0)
                    throw new ArgumentException("minSize is invalid: " + Config.MinSize);

                if (Config.MaxSize <= 0)
                    throw new ArgumentException("maxSize is invalid: " + Config.MaxSize);

                if (Config.MinSize > Config.MaxSize)
                    throw new ArgumentException("minSize is larger than maxSize. minSize: " + Config.MinSize
                            + ", maxSize: " + Config.MaxSize);

                if (Config.ObjectFactory == null)
                    throw new ArgumentNullException("objectFactory is not set");

                if (Config.OnCreate == null)
                    throw new ArgumentNullException("onCreate is null");

                if (Config.OnClose == null)
                    throw new ArgumentNullException("onClose is null");

                return (IObjectPoolConfig<T>)Config.Clone();
            }
        }
    }
}