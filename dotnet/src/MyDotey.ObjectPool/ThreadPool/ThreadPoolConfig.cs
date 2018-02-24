using System;

/**
 * @author koqizhao
 *
 * Feb 22, 2018
 */
namespace MyDotey.ObjectPool.ThreadPool
{
    public class ThreadPoolConfig : IThreadPoolConfig, ICloneable
    {
        public virtual int MinSize { get; private set; }

        public virtual int MaxSize { get; private set; }

        public virtual int QueueCapacity { get; private set; }

        protected internal ThreadPoolConfig()
        {

        }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        protected internal class Builder : AbstractBuilder<IBuilder>, IBuilder
        {

        }

        protected internal abstract class AbstractBuilder<B> : IAbstractBuilder<B>
            where B : IAbstractBuilder<B>
        {
            protected virtual ThreadPoolConfig Config { get; }

            protected AbstractBuilder()
            {
                Config = NewPoolConfig();
                Config.QueueCapacity = int.MaxValue;
            }

            protected virtual ThreadPoolConfig NewPoolConfig()
            {
                return new ThreadPoolConfig();
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

            public virtual B SetQueueCapacity(int queueCapacity)
            {
                Config.QueueCapacity = queueCapacity;
                return (B)(object)this;
            }

            public virtual IThreadPoolConfig Build()
            {
                if (Config.MinSize < 0)
                    throw new ArgumentException("minSize is invalid: " + Config.MinSize);

                if (Config.MaxSize <= 0)
                    throw new ArgumentException("maxSize is invalid: " + Config.MaxSize);

                if (Config.MinSize > Config.MaxSize)
                    throw new ArgumentException("minSize is larger than maxSize. minSize: " + Config.MinSize
                            + ", maxSize: " + Config.MaxSize);

                if (Config.QueueCapacity < 0)
                    throw new ArgumentException("queueCapacity is less than 0");

                return (IThreadPoolConfig)Config.Clone();
            }
        }
    }
}