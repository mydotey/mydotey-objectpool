package org.mydotey.objectpool.threadpool;

import java.io.IOException;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.atomic.AtomicInteger;

import org.junit.Assert;
import org.junit.Ignore;
import org.junit.Test;
import org.mydotey.objectpool.ObjectPool;
import org.mydotey.objectpool.facade.ThreadPools;
import org.mydotey.objectpool.threadpool.DefaultThreadPool;
import org.mydotey.objectpool.threadpool.ThreadPool;
import org.mydotey.objectpool.threadpool.ThreadPoolConfig;

/**
 * @author koqizhao
 *
 * Feb 6, 2018
 */
public class ObjectPoolTest {

    protected int _minSize = 10;
    protected int _maxSize = 100;
    protected int _submitterThreadCount = 10;

    protected int _defaultTaskCount = _minSize;
    protected long _defaultTaskSleep = 0;
    protected long _defaultViInitDelay = 1000;
    protected int _defaultSizeAfterSumit = -1;
    protected long _defaultFinishSleep = 0;
    protected int _defaultFinalSize = -1;

    protected ThreadPool newThreadPool() {
        ThreadPoolConfig.Builder builder = ThreadPools.newThreadPoolConfigBuilder();
        builder.setMinSize(_minSize).setMaxSize(_maxSize);
        return ThreadPools.newThreadPool(builder.build());
    }

    @Test
    public void threadPoolCreateTest() throws IOException {
        System.out.println();
        System.out.println();

        ThreadPool pool = newThreadPool();
        System.out.println("Pool Size: " + pool.getSize());
        Assert.assertEquals(_minSize, pool.getSize());
        pool.close();
    }

    @Test
    public void threadPoolSubmitTaskTest() throws IOException, InterruptedException {
        int taskCount = _minSize;
        long taskSleep = _defaultTaskSleep;
        long viInitDelay = _defaultViInitDelay;
        int sizeAfterSubmit = _minSize;
        long finishSleep = 1;
        int finalSize = _defaultFinalSize;
        threadPoolSubmitTaskTest(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize);
    }

    @Test
    public void threadPoolSubmitTaskTest2() throws IOException, InterruptedException {
        int taskCount = _minSize;
        long taskSleep = 10;
        long viInitDelay = _defaultViInitDelay;
        int sizeAfterSubmit = _minSize;
        long finishSleep = 1000;
        int finalSize = _minSize;
        threadPoolSubmitTaskTest(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize);
    }

    @Test
    public void threadPoolSubmitTaskTest3() throws IOException, InterruptedException {
        int taskCount = 50;
        long taskSleep = 50;
        long viInitDelay = _defaultViInitDelay;
        int sizeAfterSubmit = _defaultSizeAfterSumit;
        long finishSleep = 1000;
        int finalSize = _defaultFinalSize;
        threadPoolSubmitTaskTest(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize);
    }

    @Test
    public void threadPoolSubmitTaskTest4() throws IOException, InterruptedException {
        int taskCount = _maxSize;
        long taskSleep = 500;
        long viInitDelay = _defaultViInitDelay;
        int sizeAfterSubmit = _maxSize;
        long finishSleep = 5000;
        int finalSize = _maxSize;
        threadPoolSubmitTaskTest(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize);
    }

    @Test
    public void threadPoolSubmitTaskTest5() throws IOException, InterruptedException {
        int taskCount = 200;
        long taskSleep = 50;
        long viInitDelay = _defaultViInitDelay;
        int sizeAfterSubmit = _maxSize;
        long finishSleep = 1000;
        int finalSize = _maxSize;
        threadPoolSubmitTaskTest(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize);
    }

    @Test
    public void threadPoolSubmitTaskTest6() throws IOException, InterruptedException {
        int taskCount = 200;
        long taskSleep = 2000;
        long viInitDelay = _defaultViInitDelay;
        int sizeAfterSubmit = _maxSize;
        long finishSleep = 5000;
        int finalSize = _maxSize;
        threadPoolSubmitTaskTest(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize);
    }

    @Test
    @Ignore
    public void threadPoolSubmitTaskStressTest() throws IOException, InterruptedException {
        int count = 100;
        for (int i = 0; i < count; i++)
            threadPoolSubmitTaskTest();
    }

    @Test
    @Ignore
    public void threadPoolSubmitTaskStressTest2() throws IOException, InterruptedException {
        int count = 100;
        for (int i = 0; i < count; i++)
            threadPoolSubmitTaskTest4();
    }

    @Test
    public void threadPoolSubmitTaskConcurrentTest() throws IOException, InterruptedException {
        int taskCount = 200;
        long taskSleep = 2000;
        long viInitDelay = _defaultViInitDelay;
        int sizeAfterSubmit = _defaultSizeAfterSumit;
        long finishSleep = 5000;
        int finalSize = _maxSize;
        threadPoolSubmitTaskTest(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize,
                newThreadPool(), SubmissionMode.Concurrent);
    }

    @Test
    public void threadPoolSubmitTaskConcurrentTest2() throws IOException, InterruptedException {
        int taskCount = 200;
        long taskSleep = 2000;
        long viInitDelay = _defaultViInitDelay;
        int sizeAfterSubmit = _defaultSizeAfterSumit;
        long finishSleep = 5000;
        int finalSize = _maxSize;
        threadPoolSubmitTaskTest(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize,
                newThreadPool(), SubmissionMode.SelfSelf);
    }

    protected void threadPoolSubmitTaskTest(int taskCount, long taskSleep, long viInitDelay, int sizeAfterSubmit,
            long finishSleep, int finalSize) throws IOException, InterruptedException {
        threadPoolSubmitTaskTest(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize,
                newThreadPool());
    }

    protected void threadPoolSubmitTaskTest(int taskCount, long taskSleep, long viInitDelay, int sizeAfterSubmit,
            long finishSleep, int finalSize, ThreadPool customPool) throws IOException, InterruptedException {
        threadPoolSubmitTaskTest(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize, customPool,
                SubmissionMode.SingleThread);
    }

    protected void threadPoolSubmitTaskTest(int taskCount, long taskSleep, long viInitDelay, int sizeAfterSubmit,
            long finishSleep, int finalSize, ThreadPool customPool, SubmissionMode submissionMode)
            throws IOException, InterruptedException {
        System.out.println();
        System.out.println();

        AtomicInteger counter = new AtomicInteger();
        CountDownLatch countDownLatch = new CountDownLatch(taskCount);
        Runnable task = () -> {
            counter.incrementAndGet();
            countDownLatch.countDown();
            if (taskSleep <= 0)
                return;

            try {
                Thread.sleep(taskSleep);
            } catch (Exception e) {
            }
        };
        long now = System.currentTimeMillis();
        ExecutorService taskSubmitter = null;
        ScheduledExecutorService viTimer = null;
        ThreadPool self = null;
        try (ThreadPool pool = customPool) {
            System.out.println("new thread pool eclipsed: " + (System.currentTimeMillis() - now));
            System.out.println("counter value: " + counter);
            vi(pool);
            Assert.assertEquals(0, counter.get());

            viTimer = Executors.newSingleThreadScheduledExecutor();
            viTimer.scheduleWithFixedDelay(() -> {
                System.out.println("counter value: " + counter);
                vi(pool);
            }, 1000, 500, TimeUnit.MILLISECONDS);

            switch (submissionMode) {
                case SingleThread:
                    for (int i = 0; i < taskCount; i++)
                        pool.submit(task);
                    break;
                case Concurrent:
                    taskSubmitter = Executors.newFixedThreadPool(_submitterThreadCount);
                    now = System.currentTimeMillis();
                    for (int i = 0; i < taskCount; i++)
                        taskSubmitter.submit(() -> {
                            try {
                                pool.submit(task);
                            } catch (InterruptedException e) {
                                e.printStackTrace();
                            }
                        });
                    break;
                case SelfSelf:
                    self = newThreadPool();
                    for (int i = 0; i < taskCount; i++)
                        self.submit(() -> {
                            try {
                                pool.submit(task);
                            } catch (InterruptedException e) {
                                e.printStackTrace();
                            }
                        });
                    break;
                default:
                    Assert.fail("impossible submissionMode: " + submissionMode);
                    break;
            }

            System.out.println("submit tasks eclipsed: " + (System.currentTimeMillis() - now));
            System.out.println("counter value: " + counter);
            vi(pool);

            Assert.assertTrue(_minSize <= pool.getSize());
            Assert.assertTrue(_maxSize >= pool.getSize());
            if (sizeAfterSubmit >= 0)
                Assert.assertEquals(sizeAfterSubmit, pool.getSize());

            countDownLatch.await();

            System.out.println("tasks run time: " + (System.currentTimeMillis() - now));
            System.out.println("counter value: " + counter);
            vi(pool);

            Assert.assertEquals(taskCount, counter.get());
            Assert.assertTrue(_minSize <= pool.getSize());
            Assert.assertTrue(_maxSize >= pool.getSize());

            if (finishSleep <= 0)
                return;

            Thread.sleep(finishSleep);

            vi(pool);

            Assert.assertEquals(taskCount, counter.get());
            Assert.assertTrue(_minSize <= pool.getSize());
            Assert.assertTrue(_maxSize >= pool.getSize());

            if (finalSize >= 0)
                Assert.assertEquals(finalSize, pool.getSize());
        } finally {
            if (taskSubmitter != null)
                taskSubmitter.shutdown();

            if (self != null)
                self.close();

            if (viTimer != null)
                viTimer.shutdown();
        }
    }

    protected void vi(ThreadPool pool) {
        System.out.println();
        ObjectPool<?> objectPool = ((DefaultThreadPool) pool).getObjectPool();
        System.out.println("pool size: " + objectPool.getSize());
        System.out.println("pool acquired size: " + objectPool.getAcquiredSize());
        System.out.println("pool available size: " + objectPool.getAvailableSize());
        System.out.println();
    }

    protected enum SubmissionMode {
        SingleThread, Concurrent, SelfSelf
    }
}
