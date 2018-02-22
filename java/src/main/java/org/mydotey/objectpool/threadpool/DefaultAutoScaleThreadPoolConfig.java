package org.mydotey.objectpool.threadpool;

import org.mydotey.objectpool.autoscale.DefaultAutoScaleObjectPoolConfig;

/**
 * @author koqizhao
 *
 * Feb 5, 2018
 */
public class DefaultAutoScaleThreadPoolConfig extends DefaultAutoScaleObjectPoolConfig<WorkerThread>
        implements AutoScaleThreadPoolConfig {

    protected DefaultAutoScaleThreadPoolConfig() {

    }

    public static class Builder extends DefaultAutoScaleObjectPoolConfig.Builder<WorkerThread>
            implements AutoScaleThreadPoolConfig.Builder {

        private ThreadPool _threadPool;

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

        protected Builder setThreadPool(ThreadPool pool) {
            _threadPool = pool;
            return this;
        }

        @Override
        public DefaultAutoScaleThreadPoolConfig build() {
            if (_threadPool == null)
                throw new IllegalStateException("_threadPool is null");

            DefaultThreadPool pool = (DefaultThreadPool) _threadPool;
            setObjectFactory(() -> new WorkerThread(t -> pool.getObjectPool().release(t.getPoolEntry())))
                    .setOnCreate(e -> {
                        e.getObject().setPoolEntry(e);
                        e.getObject().start();
                    }).setOnClose(e -> e.getObject().interrupt())
                    .setStaleChecker(t -> t.getState() == Thread.State.TERMINATED);
            return (DefaultAutoScaleThreadPoolConfig) super.build();
        }

    }

}
