package org.mydotey.objectpool.threadpool.autoscale;

import java.util.concurrent.TimeUnit;

import org.mydotey.objectpool.threadpool.DefaultThreadPoolConfig;

/**
 * @author koqizhao
 *
 * Feb 5, 2018
 */
public class DefaultAutoScaleThreadPoolConfig extends DefaultThreadPoolConfig implements AutoScaleThreadPoolConfig {

    private long _maxIdleTime;
    private long _checkInterval;
    private int _scaleFactor;

    protected DefaultAutoScaleThreadPoolConfig() {

    }

    @Override
    public long getMaxIdleTime() {
        return _maxIdleTime;
    }

    @Override
    public long getCheckInterval() {
        return _checkInterval;
    }

    @Override
    public int getScaleFactor() {
        return _scaleFactor;
    }

    public static class Builder extends AbstractBuilder<AutoScaleThreadPoolConfig.Builder>
            implements AutoScaleThreadPoolConfig.Builder {

    }

    @SuppressWarnings("unchecked")
    protected static abstract class AbstractBuilder<B extends AutoScaleThreadPoolConfig.AbstractBuilder<B>>
            extends DefaultThreadPoolConfig.AbstractBuilder<B> implements AutoScaleThreadPoolConfig.AbstractBuilder<B> {

        protected AbstractBuilder() {
            getPoolConfig()._maxIdleTime = Long.MAX_VALUE;
            getPoolConfig()._checkInterval = TimeUnit.SECONDS.toMillis(10);
            getPoolConfig()._scaleFactor = 1;
        }

        @Override
        protected DefaultAutoScaleThreadPoolConfig newPoolConfig() {
            return new DefaultAutoScaleThreadPoolConfig();
        }

        @Override
        protected DefaultAutoScaleThreadPoolConfig getPoolConfig() {
            return (DefaultAutoScaleThreadPoolConfig) super.getPoolConfig();
        }

        @Override
        public B setMaxIdleTime(long maxIdleTime) {
            getPoolConfig()._maxIdleTime = maxIdleTime;
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
        public DefaultAutoScaleThreadPoolConfig build() {
            if (getPoolConfig()._maxIdleTime <= 0)
                throw new IllegalArgumentException("maxIdleTime is invalid: " + getPoolConfig()._maxIdleTime);

            if (getPoolConfig()._checkInterval <= 0)
                throw new IllegalArgumentException("checkInterval is invalid: " + getPoolConfig()._checkInterval);

            if (getPoolConfig()._scaleFactor <= 0)
                throw new IllegalArgumentException("invalid scaleFactor: " + getPoolConfig()._scaleFactor);

            if (getPoolConfig()._scaleFactor - 1 > getPoolConfig().getMaxSize() - getPoolConfig().getMinSize())
                throw new IllegalArgumentException("too large scaleFactor: " + getPoolConfig()._scaleFactor);

            return (DefaultAutoScaleThreadPoolConfig) super.build();
        }

    }

}