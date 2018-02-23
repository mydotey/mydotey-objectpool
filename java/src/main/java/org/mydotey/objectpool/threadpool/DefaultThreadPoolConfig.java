package org.mydotey.objectpool.threadpool;

import org.mydotey.objectpool.DefaultObjectPoolConfig;

/**
 * @author koqizhao
 *
 * Feb 2, 2018
 */
public class DefaultThreadPoolConfig extends DefaultObjectPoolConfig<WorkerThread> implements ThreadPoolConfig {

    protected DefaultThreadPoolConfig() {

    }

    public static class Builder extends DefaultObjectPoolConfig.Builder<WorkerThread>
            implements ThreadPoolConfig.Builder {

        private ThreadPool _threadPool;

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

        protected Builder setThreadPool(ThreadPool pool) {
            _threadPool = pool;
            return this;
        }

        @Override
        public DefaultThreadPoolConfig build() {
            if (_threadPool == null)
                throw new IllegalStateException("threadPool is null");

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
