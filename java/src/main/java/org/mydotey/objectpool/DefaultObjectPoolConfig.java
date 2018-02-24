package org.mydotey.objectpool;

import java.io.Closeable;
import java.util.function.Consumer;
import java.util.function.Supplier;

import org.mydotey.objectpool.ObjectPool.Entry;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * @author koqizhao
 *
 * Feb 2, 2018
 */
public class DefaultObjectPoolConfig<T> implements ObjectPoolConfig<T>, Cloneable {

    private int _minSize;
    private int _maxSize;
    private Supplier<T> _objectFactory;
    private Consumer<Entry<T>> _onCreate;
    private Consumer<Entry<T>> _onClose;

    protected DefaultObjectPoolConfig() {

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
    public Supplier<T> getObjectFactory() {
        return _objectFactory;
    }

    @Override
    public Consumer<Entry<T>> getOnCreate() {
        return _onCreate;
    }

    @Override
    public Consumer<Entry<T>> getOnClose() {
        return _onClose;
    }

    @SuppressWarnings("unchecked")
    @Override
    protected DefaultObjectPoolConfig<T> clone() {
        try {
            return (DefaultObjectPoolConfig<T>) super.clone();
        } catch (CloneNotSupportedException e) {
            throw new UnsupportedOperationException(e);
        }
    }

    public static class Builder<T> extends AbstractBuilder<T, ObjectPoolConfig.Builder<T>>
            implements ObjectPoolConfig.Builder<T> {

    }

    @SuppressWarnings({ "unchecked", "rawtypes" })
    protected static abstract class AbstractBuilder<T, B extends ObjectPoolConfig.AbstractBuilder<T, B>>
            implements ObjectPoolConfig.AbstractBuilder<T, B> {

        private static Logger _logger = LoggerFactory.getLogger(ObjectPool.class);

        protected static final Consumer<Entry> DEFAULT_ON_CREATE = e -> {
        };

        protected static final Consumer<Entry> DEFAULT_ON_CLOSE = e -> {
            if (e.getObject() instanceof Closeable) {
                try {
                    ((Closeable) e.getObject()).close();
                } catch (Exception ex) {
                    _logger.error("close object failed", ex);
                }
            }
        };

        protected DefaultObjectPoolConfig<T> _config;

        protected AbstractBuilder() {
            _config = newPoolConfig();
            _config._onCreate = (Consumer) DEFAULT_ON_CREATE;
            _config._onClose = (Consumer) DEFAULT_ON_CLOSE;
        }

        protected DefaultObjectPoolConfig<T> newPoolConfig() {
            return new DefaultObjectPoolConfig<T>();
        }

        protected DefaultObjectPoolConfig<T> getPoolConfig() {
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
        public B setObjectFactory(Supplier<T> objectFactory) {
            _config._objectFactory = objectFactory;
            return (B) this;
        }

        @Override
        public B setOnCreate(Consumer<Entry<T>> onCreate) {
            _config._onCreate = onCreate;
            return (B) this;
        }

        @Override
        public B setOnClose(Consumer<Entry<T>> onClose) {
            _config._onClose = onClose;
            return (B) this;
        }

        @Override
        public ObjectPoolConfig<T> build() {
            if (_config._minSize < 0)
                throw new IllegalArgumentException("minSize is invalid: " + _config._minSize);

            if (_config._maxSize <= 0)
                throw new IllegalArgumentException("maxSize is invalid: " + _config._maxSize);

            if (_config._minSize > _config._maxSize)
                throw new IllegalArgumentException("minSize is larger than maxSiz. minSize: " + _config._minSize
                        + ", maxSize: " + _config._maxSize);

            if (_config._objectFactory == null)
                throw new IllegalArgumentException("objectFactory is not set");

            if (_config._onCreate == null)
                throw new IllegalArgumentException("onCreate is null");

            if (_config._onClose == null)
                throw new IllegalArgumentException("onClose is null");

            return _config.clone();
        }

    }

}
