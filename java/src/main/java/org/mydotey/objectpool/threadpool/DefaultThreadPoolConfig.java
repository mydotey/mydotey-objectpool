package org.mydotey.objectpool.threadpool;

import org.mydotey.objectpool.DefaultObjectPoolConfig;

/**
 * @author koqizhao
 *
 * Feb 2, 2018
 */
public class DefaultThreadPoolConfig extends DefaultObjectPoolConfig<WorkerThread> implements ThreadPoolConfig {

    private int _queueCapacity;

    protected DefaultThreadPoolConfig() {

    }

    @Override
    public int getQueueCapacity() {
        return _queueCapacity;
    }

    public static class Builder extends DefaultObjectPoolConfig.Builder<WorkerThread>
            implements ThreadPoolConfig.Builder {

        public Builder() {
            setQueueCapacity(Integer.MAX_VALUE);
        }

        @Override
        protected DefaultThreadPoolConfig newPoolConfig() {
            return new DefaultThreadPoolConfig();
        }

        @Override
        protected DefaultThreadPoolConfig getPoolConfig() {
            return (DefaultThreadPoolConfig) super.getPoolConfig();
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
        public DefaultThreadPoolConfig build() {
            if (getPoolConfig()._queueCapacity < 0)
                throw new IllegalArgumentException("queueCapacity is less than 0");

            return (DefaultThreadPoolConfig) super.build();
        }

    }

}
