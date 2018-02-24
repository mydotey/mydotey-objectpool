package org.mydotey.objectpool.threadpool;

import org.mydotey.objectpool.DefaultObjectPoolConfig;

/**
 * @author koqizhao
 *
 * Feb 2, 2018
 */
public class DefaultThreadPoolConfig extends DefaultObjectPoolConfig<WorkerThread> implements ThreadPoolConfig {

    protected int _queueCapacity;

    protected DefaultThreadPoolConfig() {

    }

    @Override
    public int getQueueCapacity() {
        return _queueCapacity;
    }

    public static class Builder extends DefaultObjectPoolConfig.Builder<WorkerThread>
            implements ThreadPoolConfig.Builder {

        private ThreadPool _threadPool;

        public Builder() {
            setQueueCapacity(Integer.MAX_VALUE);
        }

        @Override
        protected DefaultThreadPoolConfig newPoolConfig() {
            return new DefaultThreadPoolConfig();
        }

        @Override
        protected DefaultThreadPoolConfig getPoolConfig() {
            return (DefaultThreadPoolConfig) _config;
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

        protected Builder setThreadPool(ThreadPool pool) {
            _threadPool = pool;
            return this;
        }

        @Override
        public DefaultThreadPoolConfig build() {
            if (getPoolConfig()._queueCapacity < 0)
                throw new IllegalArgumentException("queueCapacity is less than 0");

            if (_threadPool == null)
                throw new IllegalArgumentException("threadPool is null");

            DefaultThreadPool pool = (DefaultThreadPool) _threadPool;
            setObjectFactory(() -> new WorkerThread(t -> pool.getObjectPool().release(t.getPoolEntry())))
                    .setOnCreate(e -> {
                        e.getObject().setPoolEntry(e);
                        e.getObject().start();
                    }).setOnClose(e -> e.getObject().interrupt());
            return (DefaultThreadPoolConfig) super.build();
        }

    }

}
