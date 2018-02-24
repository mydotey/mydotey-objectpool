package org.mydotey.objectpool.threadpool.autoscale;

import org.mydotey.objectpool.autoscale.DefaultAutoScaleObjectPoolConfig;
import org.mydotey.objectpool.threadpool.ThreadPool;
import org.mydotey.objectpool.threadpool.WorkerThread;

/**
 * @author koqizhao
 *
 * Feb 5, 2018
 */
public class DefaultAutoScaleThreadPoolConfig extends DefaultAutoScaleObjectPoolConfig<WorkerThread>
        implements AutoScaleThreadPoolConfig {

    protected int _queueCapacity;

    protected DefaultAutoScaleThreadPoolConfig() {

    }

    @Override
    public int getQueueCapacity() {
        return _queueCapacity;
    }

    public static class Builder extends DefaultAutoScaleObjectPoolConfig.Builder<WorkerThread>
            implements AutoScaleThreadPoolConfig.Builder {

        private ThreadPool _threadPool;

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

        protected Builder setThreadPool(ThreadPool pool) {
            _threadPool = pool;
            return this;
        }

        @Override
        public DefaultAutoScaleThreadPoolConfig build() {
            if (getPoolConfig()._queueCapacity < 0)
                throw new IllegalArgumentException("queueCapacity is less than 0");

            if (_threadPool == null)
                throw new IllegalArgumentException("threadPool is null");

            DefaultAutoScaleThreadPool pool = (DefaultAutoScaleThreadPool) _threadPool;
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
