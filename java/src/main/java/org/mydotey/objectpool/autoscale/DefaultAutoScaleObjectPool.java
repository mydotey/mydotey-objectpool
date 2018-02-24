package org.mydotey.objectpool.autoscale;

import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.atomic.AtomicBoolean;

import org.mydotey.objectpool.DefaultObjectPool;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * @author koqizhao
 *
 * Feb 5, 2018
 */
public class DefaultAutoScaleObjectPool<T> extends DefaultObjectPool<T> implements AutoScaleObjectPool<T> {

    private static Logger _logger = LoggerFactory.getLogger(DefaultAutoScaleObjectPool.class);

    protected ScheduledExecutorService _taskScheduler;

    protected AtomicBoolean _scalingOut;

    protected Runnable _scaleOutTask = () -> {
        try {
            tryAddNewEntry(getConfig().getScaleFactor() - 1);
            _logger.info("scaleOut success");
        } catch (Exception ex) {
            _logger.error("scaleOut failed", ex);
        } finally {
            _scalingOut.set(false);
        }
    };

    public DefaultAutoScaleObjectPool(AutoScaleObjectPoolConfig<T> config) {
        super(config);
    }

    @Override
    protected void init() {
        super.init();

        _taskScheduler = Executors.newSingleThreadScheduledExecutor();
        _taskScheduler.scheduleWithFixedDelay(() -> DefaultAutoScaleObjectPool.this.autoCheck(),
                getConfig().getCheckInterval(), getConfig().getCheckInterval(), TimeUnit.MILLISECONDS);

        _scalingOut = new AtomicBoolean();
    }

    @Override
    protected DefaultEntry<T> tryAddNewEntryAndAcquireOne() {
        DefaultEntry<T> entry = tryCreateNewEntry();
        if (entry == null)
            return null;

        if (_scalingOut.compareAndSet(false, true))
            submitTaskSafe(_scaleOutTask);

        return super.doAcquire(entry);
    }

    @Override
    protected void addNewEntry(DefaultEntry<T> entry) {
        synchronized (((AutoScaleEntry<T>) entry).getKey()) {
            super.addNewEntry(entry);
        }
    }

    @Override
    protected AutoScaleEntry<T> newPoolEntry(Object key) {
        return (AutoScaleEntry<T>) super.newPoolEntry(key);
    }

    @Override
    protected AutoScaleEntry<T> newConcretePoolEntry(Object key, T obj) {
        return new AutoScaleEntry<T>(key, obj);
    }

    @Override
    protected AutoScaleEntry<T> getEntry(Object key) {
        return (AutoScaleEntry<T>) super.getEntry(key);
    }

    @Override
    public AutoScaleObjectPoolConfig<T> getConfig() {
        return (AutoScaleObjectPoolConfig<T>) super.getConfig();
    }

    @Override
    protected AutoScaleEntry<T> tryAcquire(Object key) {
        AutoScaleEntry<T> entry = doAcquire(key);
        if (entry != null)
            return entry;

        return (AutoScaleEntry<T>) tryAcquire();
    }

    @Override
    protected AutoScaleEntry<T> acquire(Object key) throws InterruptedException {
        AutoScaleEntry<T> entry = doAcquire(key);
        if (entry != null)
            return entry;

        return (AutoScaleEntry<T>) acquire();
    }

    @Override
    protected AutoScaleEntry<T> doAcquire(Object key) {
        synchronized (key) {
            AutoScaleEntry<T> entry = getEntry(key);
            if (entry == null)
                return null;

            super.doAcquire(entry);

            if (!needRefresh(entry)) {
                entry.renew();
                return entry;
            } else {
                entry.setStatus(AutoScaleEntry.Status.PENDING_REFRESH);
            }
        }

        releaseKey(key);
        return null;
    }

    @Override
    protected void releaseKey(Object key) {
        submitTaskSafe(() -> doReleaseKey(key));
    }

    protected void doReleaseKey(Object key) {
        synchronized (key) {
            AutoScaleEntry<T> entry = getEntry(key);
            if (entry.getStatus() == AutoScaleEntry.Status.PENDING_REFRESH) {
                if (!tryRefresh(entry)) {
                    scaleIn(entry);
                    return;
                }
            } else
                entry.renew();

            super.releaseKey(key);
        }
    }

    protected void autoCheck() {
        for (Object key : _entries.keySet()) {
            if (tryScaleIn(key))
                continue;

            tryRefresh(key);
        }
    }

    protected boolean tryScaleIn(Object key) {
        AutoScaleEntry<T> entry = getEntry(key);
        if (!needScaleIn(entry))
            return false;

        synchronized (key) {
            entry = getEntry(key);
            if (!needScaleIn(entry))
                return false;

            scaleIn(entry);
            return true;
        }
    }

    protected void scaleIn(AutoScaleEntry<T> entry) {
        synchronized (_addLock) {
            _entries.remove(entry.getKey());
            close(entry);
            _logger.info("scaled in an object: {}", entry.getObject());
        }
    }

    protected boolean tryRefresh(Object key) {
        AutoScaleEntry<T> entry = getEntry(key);
        if (!needRefresh(entry))
            return false;

        synchronized (key) {
            entry = getEntry(key);
            if (!needRefresh(entry))
                return false;

            if (entry.getStatus() == AutoScaleEntry.Status.AVAILABLE)
                return tryRefresh(entry);

            entry.setStatus(AutoScaleEntry.Status.PENDING_REFRESH);
            return false;
        }
    }

    protected boolean tryRefresh(AutoScaleEntry<T> entry) {
        AutoScaleEntry<T> newEntry = null;
        try {
            newEntry = newPoolEntry(entry.getKey());
        } catch (Exception e) {
            String errorMessage = String.format("failed to refresh object: %s, still use it", entry.getObject());
            _logger.error(errorMessage, e);
            return false;
        }

        close(entry);
        _entries.put(entry.getKey(), newEntry);

        _logger.info("refreshed an object, old: {}, new: {}", entry.getObject(), newEntry.getObject());
        return true;
    }

    protected boolean needRefresh(AutoScaleEntry<T> entry) {
        return isExpired(entry) || isStale(entry);
    }

    protected boolean isExpired(AutoScaleEntry<T> entry) {
        return entry.getCreationTime() <= System.currentTimeMillis() - getConfig().getObjectTtl();
    }

    protected boolean isStale(AutoScaleEntry<T> entry) {
        try {
            return getConfig().getStaleChecker().apply(entry.getObject());
        } catch (Exception e) {
            _logger.error("staleChecker failed, ignore", e);
            return false;
        }
    }

    protected boolean needScaleIn(AutoScaleEntry<T> entry) {
        return entry.getStatus() == AutoScaleEntry.Status.AVAILABLE
                && entry.getLastUsedTime() <= System.currentTimeMillis() - getConfig().getMaxIdleTime()
                && getSize() > getConfig().getMinSize();
    }

    @Override
    public void doClose() {
        super.doClose();

        try {
            _taskScheduler.shutdown();
        } catch (Exception e) {
            _logger.error("shutdown task scheduler failed.", e);
        }
    }

    protected void submitTaskSafe(Runnable task) {
        try {
            _taskScheduler.submit(task);
        } catch (Exception ex) {
            task.run();
        }
    }

    protected static class AutoScaleEntry<T> extends DefaultEntry<T> {

        protected interface Status extends DefaultEntry.Status {
            String PENDING_REFRESH = "pending_refresh";
        }

        private long _creationTime;
        private volatile long _lastUsedTime;

        protected AutoScaleEntry(Object key, T obj) {
            super(key, obj);

            _creationTime = System.currentTimeMillis();
            _lastUsedTime = _creationTime;
        }

        @Override
        protected Object getKey() {
            return super.getKey();
        }

        @Override
        protected String getStatus() {
            return super.getStatus();
        }

        @Override
        protected void setStatus(String status) {
            super.setStatus(status);
        }

        protected long getCreationTime() {
            return _creationTime;
        }

        protected long getLastUsedTime() {
            return _lastUsedTime;
        }

        protected void renew() {
            _lastUsedTime = System.currentTimeMillis();
        }

    }

}
