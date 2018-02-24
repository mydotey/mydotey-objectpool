using System;

/**
 * @author koqizhao
 *
 * Feb 23, 2018
 */
namespace MyDotey.ObjectPool.AutoScale
{
    public class AutoScaleObjectPoolConfig<T> : ObjectPoolConfig<T>, IAutoScaleObjectPoolConfig<T>
    {
        public virtual long ObjectTtl { get; private set; }

        public virtual long MaxIdleTime { get; private set; }

        public virtual Func<T, bool> StaleChecker { get; private set; }

        public virtual long CheckInterval { get; private set; }

        public virtual int ScaleFactor { get; private set; }

        protected AutoScaleObjectPoolConfig()
        {

        }

        protected internal new class Builder : AbstractBuilder<IBuilder<T>>, IBuilder<T>
        {

        }

        protected internal new abstract class AbstractBuilder<B> : ObjectPoolConfig<T>.AbstractBuilder<B>, IAbstractBuilder<T, B>
            where B : IAbstractBuilder<T, B>
        {
            protected static readonly Func<T, bool> DefaultStaleChecker = o => false;

            protected AbstractBuilder()
            {
                Config.ObjectTtl = long.MaxValue;
                Config.MaxIdleTime = long.MaxValue;
                Config.StaleChecker = DefaultStaleChecker;
                Config.CheckInterval = 10 * 1000;
                Config.ScaleFactor = 1;
            }

            protected new virtual AutoScaleObjectPoolConfig<T> Config { get { return (AutoScaleObjectPoolConfig<T>)base.Config; } }

            protected override ObjectPoolConfig<T> NewPoolConfig()
            {
                return new AutoScaleObjectPoolConfig<T>();
            }

            public virtual B SetObjectTtl(long objectTtl)
            {
                Config.ObjectTtl = objectTtl;
                return (B)(object)this;
            }

            public virtual B SetMaxIdleTime(long maxIdleTime)
            {
                Config.MaxIdleTime = maxIdleTime;
                return (B)(object)this;
            }

            public virtual B SetStaleChecker(Func<T, bool> staleChecker)
            {
                Config.StaleChecker = staleChecker;
                return (B)(object)this;
            }

            public virtual B SetCheckInterval(long checkInterval)
            {
                Config.CheckInterval = checkInterval;
                return (B)(object)this;
            }

            public virtual B SetScaleFactor(int scaleFactor)
            {
                Config.ScaleFactor = scaleFactor;
                return (B)(object)this;
            }

            public new virtual IAutoScaleObjectPoolConfig<T> Build()
            {
                if (Config.ObjectTtl <= 0)
                    throw new ArgumentException("objectTtl is invalid: " + Config.ObjectTtl);

                if (Config.MaxIdleTime <= 0)
                    throw new ArgumentException("maxIdleTime is invalid: " + Config.MaxIdleTime);

                if (Config.StaleChecker == null)
                    throw new ArgumentNullException("staleChecker is null.");

                if (Config.CheckInterval <= 0 || Config.CheckInterval > int.MaxValue)
                    throw new ArgumentException("checkInterval is invalid: " + Config.CheckInterval);

                if (Config.ScaleFactor <= 0)
                    throw new ArgumentException("invalid scaleFactor: " + Config.ScaleFactor);

                if (Config.ScaleFactor - 1 > Config.MaxSize - Config.MinSize)
                    throw new ArgumentException("too large scaleFactor: " + Config.ScaleFactor);

                return (IAutoScaleObjectPoolConfig<T>)base.Build();
            }
        }
    }
}