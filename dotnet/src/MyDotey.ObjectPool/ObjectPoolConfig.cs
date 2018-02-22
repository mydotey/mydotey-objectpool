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

        protected ObjectPoolConfig()
        {

        }

        public virtual int MinSize { get; protected set; }

        public virtual int MaxSize { get; protected set; }

        public virtual Func<T> ObjectFactory { get; protected set; }

        public virtual Action<IObjectPoolEntry<T>> OnCreate { get; protected set; }

        public virtual Action<IObjectPoolEntry<T>> OnClose { get; protected set; }

        public virtual Object Clone()
        {
            return MemberwiseClone();
        }

        public class ObjectPoolConfigBuilder : AbstractObjectPoolConfigBuilder<IObjectPoolConfigBuilder<T>>
            , IObjectPoolConfigBuilder<T>
        {

        }

        public abstract class AbstractObjectPoolConfigBuilder<B> : IAbstractObjectPoolConfigBuilder<T, B>
            where B : IAbstractObjectPoolConfigBuilder<T, B>
        {

            //private static Logger _logger = LoggerFactory.getLogger(ObjectPool.class);

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

            protected ObjectPoolConfig<T> _config;

            protected AbstractObjectPoolConfigBuilder()
            {
                _config = newPoolConfig();
                _config.OnCreate = DefaultOnCreate;
                _config.OnClose = DefaultOnClose;
            }

            protected ObjectPoolConfig<T> newPoolConfig()
            {
                return new ObjectPoolConfig<T>();
            }

            public virtual B SetMinSize(int minSize)
            {
                _config.MinSize = minSize;
                return (B)(Object)this;
            }

            public virtual B SetMaxSize(int maxSize)
            {
                _config.MaxSize = maxSize;
                return (B)(Object)this;
            }

            public virtual B SetObjectFactory(Func<T> objectFactory)
            {
                _config.ObjectFactory = objectFactory;
                return (B)(Object)this;
            }

            public virtual B SetOnCreate(Action<IObjectPoolEntry<T>> onCreate)
            {
                _config.OnCreate = onCreate;
                return (B)(Object)this;
            }

            public virtual B SetOnClose(Action<IObjectPoolEntry<T>> onClose)
            {
                _config.OnClose = onClose;
                return (B)(Object)this;
            }

            public virtual IObjectPoolConfig<T> Build()
            {
                if (_config.MinSize < 0)
                    throw new ArgumentException("minSize is invalid: " + _config.MinSize);

                if (_config.MaxSize <= 0)
                    throw new ArgumentException("maxSize is invalid: " + _config.MaxSize);

                if (_config.MinSize > _config.MaxSize)
                    throw new ArgumentException("minSize is larger than maxSiz. minSize: " + _config.MinSize
                            + ", maxSize: " + _config.MaxSize);

                if (_config.ObjectFactory == null)
                    throw new ArgumentNullException("objectFactory is not set");

                if (_config.OnCreate == null)
                    throw new ArgumentNullException("onCreate is null");

                if (_config.OnClose == null)
                    throw new ArgumentNullException("onClose is null");

                return (IObjectPoolConfig<T>)_config.Clone();
            }

        }

    }

}