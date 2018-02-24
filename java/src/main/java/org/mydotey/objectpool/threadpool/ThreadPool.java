package org.mydotey.objectpool.threadpool;

import java.io.Closeable;

/**
 * @author koqizhao
 *
 * Feb 8, 2018
 */
public interface ThreadPool extends Closeable {

    ThreadPoolConfig getConfig();

    int getSize();

    int getQueueSize();

    void submit(Runnable task) throws InterruptedException;

    boolean trySubmit(Runnable task);

}