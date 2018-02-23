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
        public virtual int MinSize { get; protected set; }

        public virtual int MaxSize { get; protected set; }

        public virtual Func<T> ObjectFactory { get; protected set; }

        public virtual Action<IObjectPoolEntry<T>> OnCreate { get; protected set; }

        public virtual Action<IObjectPoolEntry<T>> OnClose { get; protected set; }

        protected ObjectPoolConfig()
        {

        }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        protected internal class ObjectPoolConfigBuilder : AbstractObjectPoolConfigBuilder<IObjectPoolConfigBuilder<T>>
            , IObjectPoolConfigBuilder<T>
        {

        }

        protected internal abstract class AbstractObjectPoolConfigBuilder<B> : IAbstractObjectPoolConfigBuilder<T, B>
            where B : IAbstractObjectPoolConfigBuilder<T, B>
        {

            //private static Logger _logger = LoggerFactory.getLogger\(objectPool.class);

            public static readonly Action<IObjectPoolEntry<T>> DefaultOnCreate = e => { };

            public static readonly Action<IObjectPoolEntry<T>> DefaultOnClose = e =>
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

            protected ObjectPoolConfig<T> Config { get; }

            protected AbstractObjectPoolConfigBuilder()
            {
                Config = NewPoolConfig();
                Config.OnCreate = DefaultOnCreate;
                Config.OnClose = DefaultOnClose;
            }

            protected ObjectPoolConfig<T> NewPoolConfig()
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

            public virtual B SetOnCreate(Action<IObjectPoolEntry<T>> onCreate)
            {
                Config.OnCreate = onCreate;
                return (B)(object)this;
            }

            public virtual B SetOnClose(Action<IObjectPoolEntry<T>> onClose)
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
                    throw new ArgumentException("minSize is larger than maxSiz. minSize: " + Config.MinSize
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