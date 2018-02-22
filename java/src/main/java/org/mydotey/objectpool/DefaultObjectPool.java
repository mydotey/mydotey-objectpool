package org.mydotey.objectpool;

import java.io.IOException;
import java.util.Objects;
import java.util.concurrent.BlockingDeque;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.LinkedBlockingDeque;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.atomic.AtomicInteger;
import java.util.concurrent.atomic.AtomicLong;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * @author koqizhao
 *
 * Feb 2, 2018
 */
public class DefaultObjectPool<T> implements ObjectPool<T> {

    private static Logger _logger = LoggerFactory.getLogger(DefaultObjectPool.class);

    protected ObjectPoolConfig<T> _config;

    protected Object _addLock;
    protected volatile boolean _isClosed;

    protected KeyGenerator _keyGenerator;
    protected BlockingDeque<Object> _availableKeys;
    protected ConcurrentHashMap<Object, Entry<T>> _entries;

    protected AtomicInteger _acquiredSize;

    public DefaultObjectPool(ObjectPoolConfig<T> config) {
        Objects.requireNonNull(config, "config is null");
        _config = config;

        _addLock = new Object();
        _acquiredSize = new AtomicInteger();

        init();
    }

    protected void init() {
        _keyGenerator = new KeyGenerator();
        _availableKeys = new LinkedBlockingDeque<>(_config.getMaxSize());
        _entries = new ConcurrentHashMap<>(_config.getMaxSize());

        tryAddNewEntry(_config.getMinSize());
    }

    protected void tryAddNewEntry(int count) {
        for (int i = 0; i < count; i++)
            tryAddNewEntry();
    }

    protected DefaultEntry<T> tryAddNewEntry() {
        DefaultEntry<T> entry = tryCreateNewEntry();
        if (entry != null)
            addNewEntry(entry);

        return entry;
    }

    protected void addNewEntry(DefaultEntry<T> entry) {
        _availableKeys.addFirst(entry.getKey());
    }

    protected DefaultEntry<T> tryCreateNewEntry() {
        if (isClosed())
            return null;

        synchronized (_addLock) {
            if (isClosed())
                return null;

            if (getSize() == _config.getMaxSize())
                return null;

            DefaultEntry<T> entry = newPoolEntry();
            entry.setStatus(DefaultEntry.Status.AVAILABLE);
            _entries.put(entry.getKey(), entry);
            return entry;
        }
    }

    protected DefaultEntry<T> newPoolEntry() {
        return newPoolEntry(_keyGenerator.generateKey());
    }

    protected DefaultEntry<T> newPoolEntry(Object key) {
        DefaultEntry<T> entry = newConcretePoolEntry(key, newObject());
        try {
            _config.getOnCreate().accept(entry);
        } catch (Exception e) {
            _logger.error("onEntryCreate failed", e);
        }

        return entry;
    }

    protected DefaultEntry<T> newConcretePoolEntry(Object key, T obj) {
        return new DefaultEntry<T>(key, obj);
    }

    protected T newObject() {
        T obj = _config.getObjectFactory().get();
        if (obj == null) {
            _logger.error("object factory generated null, the object factory has bug");
            throw new IllegalStateException("object factory generated null");
        }

        _logger.info("new object created: {}", obj);
        return obj;
    }

    protected DefaultEntry<T> getEntry(Object key) {
        return (DefaultEntry<T>) _entries.get(key);
    }

    @Override
    public ObjectPoolConfig<T> getConfig() {
        return _config;
    }

    @Override
    public int getSize() {
        return _entries.size();
    }

    @Override
    public int getAcquiredSize() {
        return _acquiredSize.get();
    }

    @Override
    public int getAvailableSize() {
        int availableSize = getSize() - getAcquiredSize();
        return availableSize > 0 ? availableSize : 0;
    }

    @Override
    public boolean isClosed() {
        return _isClosed;
    }

    @Override
    public DefaultEntry<T> acquire() throws InterruptedException {
        checkClosed();

        DefaultEntry<T> entry = tryAcquire();
        if (entry != null)
            return entry;

        Object key = takeFirst();
        return acquire(key);
    }

    protected Object takeFirst() throws InterruptedException {
        while (true) {
            checkClosed();

            Object key = _availableKeys.pollFirst(1, TimeUnit.SECONDS);
            if (key != null)
                return key;
        }
    }

    protected void checkClosed() {
        if (isClosed())
            throw new IllegalStateException("object pool has been closed");
    }

    @Override
    public DefaultEntry<T> tryAcquire() {
        if (isClosed())
            return null;

        Object key = _availableKeys.pollFirst();
        if (key != null)
            return tryAcquire(key);

        return tryAddNewEntryAndAcquireOne();
    }

    protected DefaultEntry<T> tryAcquire(Object key) {
        return doAcquire(key);
    }

    protected DefaultEntry<T> acquire(Object key) throws InterruptedException {
        return doAcquire(key);
    }

    protected DefaultEntry<T> doAcquire(Object key) {
        DefaultEntry<T> entry = getEntry(key);
        return doAcquire(entry);
    }

    protected DefaultEntry<T> tryAddNewEntryAndAcquireOne() {
        DefaultEntry<T> entry = tryCreateNewEntry();
        if (entry == null)
            return null;

        return doAcquire(entry);
    }

    protected DefaultEntry<T> doAcquire(DefaultEntry<T> entry) {
        entry.setStatus(DefaultEntry.Status.ACQUIRED);
        _acquiredSize.incrementAndGet();
        return entry.clone();
    }

    @Override
    public void release(Entry<T> entry) {
        if (isClosed())
            return;

        DefaultEntry<T> defaultEntry = (DefaultEntry<T>) entry;
        if (defaultEntry == null || defaultEntry.getStatus() == DefaultEntry.Status.RELEASED)
            return;

        synchronized (defaultEntry) {
            if (defaultEntry.getStatus() == DefaultEntry.Status.RELEASED)
                return;

            defaultEntry.setStatus(DefaultEntry.Status.RELEASED);
            _acquiredSize.decrementAndGet();
        }

        releaseNumber(defaultEntry.getKey());
    }

    protected void releaseNumber(Object key) {
        getEntry(key).setStatus(DefaultEntry.Status.AVAILABLE);
        _availableKeys.addFirst(key);
    }

    @Override
    public void close() throws IOException {
        if (isClosed())
            return;

        synchronized (_addLock) {
            if (isClosed())
                return;

            _isClosed = true;
            doClose();
        }
    }

    protected void doClose() {
        for (Entry<T> entry : _entries.values()) {
            close((DefaultEntry<T>) entry);
        }

        _availableKeys.clear();
    }

    protected void close(DefaultEntry<T> entry) {
        entry.setStatus(DefaultEntry.Status.CLOSED);

        try {
            _config.getOnClose().accept(entry);
        } catch (Exception e) {
            _logger.error("close object failed", e);
        }
    }

    protected static class DefaultEntry<T> implements Entry<T>, Cloneable {

        protected interface Status {
            String AVAILABLE = "available";
            String ACQUIRED = "acquired";
            String RELEASED = "released";
            String CLOSED = "closed";
        }

        private Object _key;
        private volatile String _status;

        private T _obj;

        protected DefaultEntry(Object key, T obj) {
            _key = key;
            _obj = obj;
        }

        protected Object getKey() {
            return _key;
        }

        protected String getStatus() {
            return _status;
        }

        protected void setStatus(String status) {
            _status = status;
        }

        @Override
        public T getObject() {
            return _obj;
        }

        @SuppressWarnings("unchecked")
        @Override
        public DefaultEntry<T> clone() {
            try {
                return (DefaultEntry<T>) super.clone();
            } catch (CloneNotSupportedException e) {
                throw new UnsupportedOperationException(e);
            }
        }

    }

    protected static class KeyGenerator {

        private AtomicLong _counter = new AtomicLong();
        private static final long MAX = Long.MAX_VALUE / 2;

        public KeyGenerator() {

        }

        public Object generateKey() {
            long count = _counter.getAndIncrement();
            if (count > MAX)
                _logger.warn("{} objects created, maybe misused", count);

            return new Long(count);
        }

    }
}
