package org.mydotey.objectpool.threadpool;

/**
 * @author koqizhao
 *
 * Feb 2, 2018
 */
public class DefaultThreadPoolConfig implements ThreadPoolConfig, Cloneable {

    private int _minSize;
    private int _maxSize;
    private int _queueCapacity;

    protected DefaultThreadPoolConfig() {

    }

    @Override
    public int getMinSize() {
        return _minSize;
    }

    @Override
    public int getMaxSize() {
        return _maxSize;
    }

    @Override
    public int getQueueCapacity() {
        return _queueCapacity;
    }

    @Override
    protected DefaultThreadPoolConfig clone() {
        try {
            return (DefaultThreadPoolConfig) super.clone();
        } catch (CloneNotSupportedException e) {
            throw new UnsupportedOperationException(e);
        }
    }

    public static class Builder extends AbstractBuilder<ThreadPoolConfig.Builder> implements ThreadPoolConfig.Builder {

    }

    @SuppressWarnings("unchecked")
    protected static abstract class AbstractBuilder<B extends ThreadPoolConfig.AbstractBuilder<B>>
            implements ThreadPoolConfig.AbstractBuilder<B> {

        private DefaultThreadPoolConfig _config;

        protected AbstractBuilder() {
            _config = newPoolConfig();
            _config._queueCapacity = Integer.MAX_VALUE;
        }

        protected DefaultThreadPoolConfig newPoolConfig() {
            return new DefaultThreadPoolConfig();
        }

        protected DefaultThreadPoolConfig getPoolConfig() {
            return _config;
        }

        @Override
        public B setMinSize(int minSize) {
            _config._minSize = minSize;
            return (B) this;
        }

        @Override
        public B setMaxSize(int maxSize) {
            _config._maxSize = maxSize;
            return (B) this;
        }

        @Override
        public B setQueueCapacity(int queueCapacity) {
            _config._queueCapacity = queueCapacity;
            return (B) this;
        }

        @Override
        public DefaultThreadPoolConfig build() {
            if (_config._minSize < 0)
                throw new IllegalArgumentException("minSize is invalid: " + _config._minSize);

            if (_config._maxSize <= 0)
                throw new IllegalArgumentException("maxSize is invalid: " + _config._maxSize);

            if (_config._minSize > _config._maxSize)
                throw new IllegalArgumentException("minSize is larger than maxSize. minSize: " + _config._minSize
                        + ", maxSize: " + _config._maxSize);

            if (_config._queueCapacity < 0)
                throw new IllegalArgumentException("queueCapacity is less than 0");

            return _config.clone();
        }

    }

}