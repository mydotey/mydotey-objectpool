package org.mydotey.objectpool.threadpool;

import java.io.IOException;
import java.util.Objects;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.LinkedBlockingQueue;

import org.mydotey.objectpool.ObjectPool;
import org.mydotey.objectpool.ObjectPoolConfig;
import org.mydotey.objectpool.ObjectPool.Entry;
import org.mydotey.objectpool.facade.ObjectPools;

/**
 * @author koqizhao
 *
 * Feb 6, 2018
 */
public class DefaultThreadPool implements ThreadPool {

    private ThreadPoolConfig _config;
    protected ObjectPool<WorkerThread> _objectPool;

    protected boolean _hasQueue;
    protected BlockingQueue<Runnable> _taskQueue;
    protected Thread _taskConsumer;

    public DefaultThreadPool(ThreadPoolConfig config) {
        Objects.requireNonNull(config, "config is null");
        _config = config;
        _objectPool = newObjectPool();

        _hasQueue = _config.getQueueCapacity() > 0;
        if (_hasQueue) {
            _taskQueue = new LinkedBlockingQueue<>(_config.getQueueCapacity());
            _taskConsumer = new Thread(() -> consumeTask());
            _taskConsumer.setDaemon(true);
            _taskConsumer.start();
        }
    }

    protected ObjectPool<WorkerThread> newObjectPool() {
        ObjectPoolConfig.Builder<WorkerThread> builder = ObjectPools.newObjectPoolConfigBuilder();
        setObjectPoolConfigBuilder(builder);
        return ObjectPools.newObjectPool(builder.build());
    }

    protected void setObjectPoolConfigBuilder(ObjectPoolConfig.AbstractBuilder<WorkerThread, ?> builder) {
        builder.setMaxSize(_config.getMaxSize()).setMinSize(_config.getMinSize())
                .setObjectFactory(() -> new WorkerThread(t -> getObjectPool().release(t.getPoolEntry())))
                .setOnCreate(e -> {
                    e.getObject().setPoolEntry(e);
                    e.getObject().start();
                }).setOnClose(e -> e.getObject().interrupt());
    }

    protected ObjectPool<WorkerThread> getObjectPool() {
        return _objectPool;
    }

    @Override
    public ThreadPoolConfig getConfig() {
        return _config;
    }

    @Override
    public int getSize() {
        return getObjectPool().getSize();
    }

    @Override
    public int getQueueSize() {
        return _hasQueue ? _taskQueue.size() : 0;
    }

    @Override
    public void submit(Runnable task) throws InterruptedException {
        Objects.requireNonNull(task, "task is null");

        if (_hasQueue) {
            if (trySubmit(task))
                return;

            _taskQueue.put(task);
            return;
        }

        Entry<WorkerThread> entry = getObjectPool().acquire();
        entry.getObject().setTask(task);
    }

    @Override
    public boolean trySubmit(Runnable task) {
        Objects.requireNonNull(task, "task is null");

        Entry<WorkerThread> entry = getObjectPool().tryAcquire();
        if (entry == null)
            return false;

        entry.getObject().setTask(task);
        return true;
    }

    protected void consumeTask() {
        while (!Thread.currentThread().isInterrupted()) {
            try {
                Runnable task = _taskQueue.take();
                Entry<WorkerThread> entry = getObjectPool().acquire();
                entry.getObject().setTask(task);
            } catch (Exception e) {
                break;
            }
        }
    }

    @Override
    public void close() throws IOException {
        getObjectPool().close();

        if (_hasQueue)
            _taskConsumer.interrupt();
    }
}
