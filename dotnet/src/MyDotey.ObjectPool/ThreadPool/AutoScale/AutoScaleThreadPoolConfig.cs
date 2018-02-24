using System;

/**
 * @author koqizhao
 *
 * Feb 22, 2018
 */
namespace MyDotey.ObjectPool.ThreadPool.AutoScale
{
    public class AutoScaleThreadPoolConfig : ThreadPoolConfig, IAutoScaleThreadPoolConfig
    {
        public virtual long MaxIdleTime { get; private set; }

        public virtual long CheckInterval { get; private set; }

        public virtual int ScaleFactor { get; private set; }

        protected internal AutoScaleThreadPoolConfig()
        {

        }

        protected internal new class Builder : AbstractBuilder<IBuilder>, IBuilder
        {

        }

        protected internal new abstract class AbstractBuilder<B> : ThreadPoolConfig.AbstractBuilder<B>, IAbstractBuilder<B>
            where B : IAbstractBuilder<B>
        {
            public AbstractBuilder()
            {
                Config.MaxIdleTime = long.MaxValue;
                Config.CheckInterval = 10 * 1000;
                Config.ScaleFactor = 1;
            }

            protected override ThreadPoolConfig NewPoolConfig()
            {
                return new AutoScaleThreadPoolConfig();
            }

            protected new virtual AutoScaleThreadPoolConfig Config { get { return (AutoScaleThreadPoolConfig)base.Config; } }

            public virtual B SetMaxIdleTime(long maxIdleTime)
            {
                Config.MaxIdleTime = maxIdleTime;
                return (B)(object)this;
            }

            public virtual B SetScaleFactor(int scaleFactor)
            {
                Config.ScaleFactor = scaleFactor;
                return (B)(object)this;
            }

            public virtual B SetCheckInterval(long checkInterval)
            {
                Config.CheckInterval = checkInterval;
                return (B)(object)this;
            }

            public new virtual IAutoScaleThreadPoolConfig Build()
            {
                if (Config.MaxIdleTime <= 0)
                    throw new ArgumentException("maxIdleTime is invalid: " + Config.MaxIdleTime);

                if (Config.CheckInterval <= 0 || Config.CheckInterval > int.MaxValue)
                    throw new ArgumentException("checkInterval is invalid: " + Config.CheckInterval);

                if (Config.ScaleFactor <= 0)
                    throw new ArgumentException("invalid scaleFactor: " + Config.ScaleFactor);

                if (Config.ScaleFactor - 1 > Config.MaxSize - Config.MinSize)
                    throw new ArgumentException("too large scaleFactor: " + Config.ScaleFactor);

                return (IAutoScaleThreadPoolConfig)base.Build();
            }
        }
    }
}