package org.mydotey.objectpool.facade;

import org.mydotey.objectpool.threadpool.AutoScaleThreadPoolConfig;
import org.mydotey.objectpool.threadpool.DefaultAutoScaleThreadPoolConfig;
import org.mydotey.objectpool.threadpool.DefaultThreadPool;
import org.mydotey.objectpool.threadpool.DefaultThreadPoolConfig;
import org.mydotey.objectpool.threadpool.ThreadPool;
import org.mydotey.objectpool.threadpool.ThreadPoolConfig;

/**
 * @author koqizhao
 *
 * Feb 8, 2018
 */
public class ThreadPools {

    public static ThreadPoolConfig.Builder newThreadPoolConfigBuilder() {
        return new DefaultThreadPoolConfig.Builder();
    }

    public static ThreadPool newThreadPool(ThreadPoolConfig.Builder builder) {
        return new DefaultThreadPool(builder);
    }

    public static AutoScaleThreadPoolConfig.Builder newAutoScaleThreadPoolConfigBuilder() {
        return new DefaultAutoScaleThreadPoolConfig.Builder();
    }

    public static ThreadPool newAutoScaleThreadPool(AutoScaleThreadPoolConfig.Builder builder) {
        return new DefaultThreadPool(builder);
    }

}
