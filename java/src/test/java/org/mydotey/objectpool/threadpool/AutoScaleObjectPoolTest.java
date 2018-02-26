package org.mydotey.objectpool.threadpool;

import java.io.IOException;
import java.util.concurrent.TimeUnit;

import org.junit.Test;
import org.mydotey.objectpool.ObjectPoolConfig;
import org.mydotey.objectpool.autoscale.AutoScaleObjectPoolConfig;
import org.mydotey.objectpool.facade.ThreadPools;
import org.mydotey.objectpool.threadpool.ThreadPool;
import org.mydotey.objectpool.threadpool.WorkerThread;
import org.mydotey.objectpool.threadpool.autoscale.AutoScaleThreadPoolConfig;
import org.mydotey.objectpool.threadpool.autoscale.DefaultAutoScaleThreadPool;

/**
 * @author koqizhao
 *
 * Feb 6, 2018
 */
public class AutoScaleObjectPoolTest extends ObjectPoolTest {

    @Override
    protected ThreadPool newThreadPool(int queueCapacity) {
        return newThreadPool(TimeUnit.SECONDS.toMillis(2), TimeUnit.SECONDS.toMillis(2), TimeUnit.SECONDS.toMillis(10),
                queueCapacity);
    }

    protected ThreadPool newThreadPool(long checkInterval, long maxIdleTime, long ttl) {
        return newThreadPool(checkInterval, maxIdleTime, ttl, 0);
    }

    @SuppressWarnings("unchecked")
    protected ThreadPool newThreadPool(long checkInterval, long maxIdleTime, long ttl, int queueCapacity) {
        AutoScaleThreadPoolConfig.Builder builder = ThreadPools.newAutoScaleThreadPoolConfigBuilder();
        builder.setMinSize(_minSize).setMaxSize(_maxSize).setScaleFactor(5).setCheckInterval(checkInterval)
                .setMaxIdleTime(maxIdleTime).setQueueCapacity(queueCapacity);
        return new DefaultAutoScaleThreadPool(builder.build()) {
            @Override
            protected void setObjectPoolConfigBuilder(ObjectPoolConfig.AbstractBuilder<WorkerThread, ?> builder) {
                super.setObjectPoolConfigBuilder(builder);
                ((AutoScaleObjectPoolConfig.Builder<WorkerThread>) builder).setObjectTtl(ttl);
            }
        };
    }

    @Override
    public void threadPoolSubmitTaskTest4() throws IOException, InterruptedException {
        int taskCount = _maxSize;
        long taskSleep = 500;
        long viInitDelay = _defaultViInitDelay;
        int sizeAfterSubmit = _maxSize;
        long finishSleep = 5000;
        int finalSize = _minSize;
        threadPoolSubmitTaskTest(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize);
    }

    @Override
    public void threadPoolSubmitTaskTest6() throws IOException, InterruptedException {
        int taskCount = 200;
        long taskSleep = 2000;
        long viInitDelay = _defaultViInitDelay;
        int sizeAfterSubmit = _maxSize;
        long finishSleep = 10000;
        int finalSize = _minSize;
        threadPoolSubmitTaskTest(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize);
    }

    @Override
    public void threadPoolSubmitTaskTest7() throws IOException, InterruptedException {
        int taskCount = 200;
        long taskSleep = 2000;
        long viInitDelay = _defaultViInitDelay;
        int sizeAfterSubmit = _maxSize;
        long finishSleep = 10000;
        int finalSize = _minSize;
        int queueCapacity = 10;
        threadPoolSubmitTaskTest(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize,
                newThreadPool(queueCapacity));
    }

    @Test
    public void threadPoolSubmitTaskMoreTest() throws IOException, InterruptedException {
        int taskCount = 200;
        long taskSleep = 2000;
        long viInitDelay = _defaultViInitDelay;
        int sizeAfterSubmit = _maxSize;
        long finishSleep = 20000;
        int finalSize = _maxSize;
        long checkInterval = 2000;
        long maxIdleTime = 10000;
        long ttl = 5000;
        ThreadPool pool = newThreadPool(checkInterval, maxIdleTime, ttl);
        threadPoolSubmitTaskTest(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize, pool);
    }

    @Test
    public void threadPoolSubmitTaskMoreTest2() throws IOException, InterruptedException {
        int taskCount = 200;
        long taskSleep = 2000;
        long viInitDelay = _defaultViInitDelay;
        int sizeAfterSubmit = _maxSize;
        long finishSleep = 10000;
        int finalSize = _maxSize;
        long checkInterval = 2000;
        long maxIdleTime = Long.MAX_VALUE;
        long ttl = 10000;
        ThreadPool pool = newThreadPool(checkInterval, maxIdleTime, ttl);
        threadPoolSubmitTaskTest(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize, pool);
    }

    @Test
    public void threadPoolSubmitTaskMoreTest3() throws IOException, InterruptedException {
        int taskCount = 200;
        long taskSleep = 2000;
        long viInitDelay = _defaultViInitDelay;
        int sizeAfterSubmit = _maxSize;
        long finishSleep = 20000;
        int finalSize = _minSize;
        long checkInterval = 2000;
        long maxIdleTime = 10000;
        long ttl = Long.MAX_VALUE;
        ThreadPool pool = newThreadPool(checkInterval, maxIdleTime, ttl);
        threadPoolSubmitTaskTest(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize, pool);
    }

    @Override
    public void threadPoolSubmitTaskConcurrentTest() throws IOException, InterruptedException {
        int taskCount = 200;
        long taskSleep = 2000;
        long viInitDelay = _defaultViInitDelay;
        int sizeAfterSubmit = _defaultSizeAfterSubmit;
        long finishSleep = 10000;
        int finalSize = _minSize;
        threadPoolSubmitTaskTest(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize,
                newThreadPool(), SubmissionMode.Concurrent);
    }

    @Override
    public void threadPoolSubmitTaskConcurrentTest2() throws IOException, InterruptedException {
        int taskCount = 200;
        long taskSleep = 2000;
        long viInitDelay = _defaultViInitDelay;
        int sizeAfterSubmit = _defaultSizeAfterSubmit;
        long finishSleep = 10000;
        int finalSize = _minSize;
        threadPoolSubmitTaskTest(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize,
                newThreadPool(), SubmissionMode.SelfSelf);
    }

}
