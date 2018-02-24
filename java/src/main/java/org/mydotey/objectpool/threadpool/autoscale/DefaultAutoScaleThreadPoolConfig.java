package org.mydotey.objectpool.threadpool.autoscale;

import org.mydotey.objectpool.autoscale.DefaultAutoScaleObjectPoolConfig;
import org.mydotey.objectpool.threadpool.WorkerThread;

/**
 * @author koqizhao
 *
 * Feb 5, 2018
 */
public class DefaultAutoScaleThreadPoolConfig extends DefaultAutoScaleObjectPoolConfig<WorkerThread>
        implements AutoScaleThreadPoolConfig {

    private int _queueCapacity;

    protected DefaultAutoScaleThreadPoolConfig() {

    }

    @Override
    public int getQueueCapacity() {
        return _queueCapacity;
    }

    public static class Builder extends DefaultAutoScaleObjectPoolConfig.Builder<WorkerThread>
            implements AutoScaleThreadPoolConfig.Builder {

        public Builder() {
            setQueueCapacity(Integer.MAX_VALUE);
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
        public Builder setMinSize(int minSize) {
            return (Builder) super.setMinSize(minSize);
        }

        @Override
        public Builder setMaxSize(int maxSize) {
            return (Builder) super.setMaxSize(maxSize);
        }

        @Override
        public Builder setQueueCapacity(int queueCapacity) {
            getPoolConfig()._queueCapacity = queueCapacity;
            return this;
        }

        @Override
        public Builder setMaxIdleTime(long maxIdleTime) {
            return (Builder) super.setMaxIdleTime(maxIdleTime);
        }

        @Override
        public Builder setScaleFactor(int scaleFactor) {
            return (Builder) super.setScaleFactor(scaleFactor);
        }

        @Override
        public Builder setCheckInterval(long checkInterval) {
            return (Builder) super.setCheckInterval(checkInterval);
        }

        @Override
        public DefaultAutoScaleThreadPoolConfig build() {
            if (getPoolConfig()._queueCapacity < 0)
                throw new IllegalArgumentException("queueCapacity is less than 0");

            return (DefaultAutoScaleThreadPoolConfig) super.build();
        }

    }

}
