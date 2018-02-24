package org.mydotey.objectpool.autoscale;

import java.util.concurrent.TimeUnit;
import java.util.function.Function;

import org.mydotey.objectpool.DefaultObjectPoolConfig;

/**
 * @author koqizhao
 *
 * Feb 5, 2018
 */
public class DefaultAutoScaleObjectPoolConfig<T> extends DefaultObjectPoolConfig<T>
        implements AutoScaleObjectPoolConfig<T> {

    private long _objectTtl;
    private long _maxIdleTime;
    private Function<T, Boolean> _staleChecker;
    private long _checkInterval;
    private int _scaleFactor;

    protected DefaultAutoScaleObjectPoolConfig() {

    }

    @Override
    public long getObjectTtl() {
        return _objectTtl;
    }

    @Override
    public long getMaxIdleTime() {
        return _maxIdleTime;
    }

    @Override
    public Function<T, Boolean> getStaleChecker() {
        return _staleChecker;
    }

    @Override
    public long getCheckInterval() {
        return _checkInterval;
    }

    @Override
    public int getScaleFactor() {
        return _scaleFactor;
    }

    public static class Builder<T> extends AbstractBuilder<T, AutoScaleObjectPoolConfig.Builder<T>>
            implements AutoScaleObjectPoolConfig.Builder<T> {

    }

    @SuppressWarnings({ "unchecked", "rawtypes" })
    protected static abstract class AbstractBuilder<T, B extends AutoScaleObjectPoolConfig.AbstractBuilder<T, B>>
            extends DefaultObjectPoolConfig.AbstractBuilder<T, B>
            implements AutoScaleObjectPoolConfig.AbstractBuilder<T, B> {

        protected static final Function DEFAULT_STALE_CHECKER = o -> false;

        protected AbstractBuilder() {
            getPoolConfig()._objectTtl = Long.MAX_VALUE;
            getPoolConfig()._maxIdleTime = Long.MAX_VALUE;
            getPoolConfig()._staleChecker = DEFAULT_STALE_CHECKER;
            getPoolConfig()._checkInterval = TimeUnit.SECONDS.toMillis(10);
            getPoolConfig()._scaleFactor = 1;
        }

        @Override
        protected DefaultAutoScaleObjectPoolConfig<T> newPoolConfig() {
            return new DefaultAutoScaleObjectPoolConfig<T>();
        }

        @Override
        protected DefaultAutoScaleObjectPoolConfig<T> getPoolConfig() {
            return (DefaultAutoScaleObjectPoolConfig<T>) super.getPoolConfig();
        }

        @Override
        public B setObjectTtl(long objectTtl) {
            getPoolConfig()._objectTtl = objectTtl;
            return (B) this;
        }

        @Override
        public B setMaxIdleTime(long maxIdleTime) {
            getPoolConfig()._maxIdleTime = maxIdleTime;
            return (B) this;
        }

        @Override
        public B setStaleChecker(Function<T, Boolean> staleChecker) {
            getPoolConfig()._staleChecker = staleChecker;
            return (B) this;
        }

        @Override
        public B setCheckInterval(long checkInterval) {
            getPoolConfig()._checkInterval = checkInterval;
            return (B) this;
        }

        @Override
        public B setScaleFactor(int scaleFactor) {
            getPoolConfig()._scaleFactor = scaleFactor;
            return (B) this;
        }

        @Override
        public DefaultAutoScaleObjectPoolConfig<T> build() {
            if (getPoolConfig()._objectTtl <= 0)
                throw new IllegalArgumentException("objectTtl is invalid: " + getPoolConfig()._objectTtl);

            if (getPoolConfig()._maxIdleTime <= 0)
                throw new IllegalArgumentException("maxIdleTime is invalid: " + getPoolConfig()._maxIdleTime);

            if (getPoolConfig()._staleChecker == null)
                throw new IllegalArgumentException("staleChecker is null.");

            if (getPoolConfig()._checkInterval <= 0)
                throw new IllegalArgumentException("checkInterval is invalid: " + getPoolConfig()._checkInterval);

            if (getPoolConfig()._scaleFactor <= 0)
                throw new IllegalArgumentException("invalid scaleFactor: " + getPoolConfig()._scaleFactor);

            if (getPoolConfig()._scaleFactor - 1 > getPoolConfig().getMaxSize() - getPoolConfig().getMinSize())
                throw new IllegalArgumentException("too large scaleFactor: " + getPoolConfig()._scaleFactor);

            return (DefaultAutoScaleObjectPoolConfig<T>) super.build();
        }

    }

}
