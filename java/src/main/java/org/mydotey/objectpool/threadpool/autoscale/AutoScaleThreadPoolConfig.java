package org.mydotey.objectpool.threadpool.autoscale;

import org.mydotey.objectpool.threadpool.ThreadPoolConfig;

/**
 * @author koqizhao
 *
 * Feb 6, 2018
 */
public interface AutoScaleThreadPoolConfig extends ThreadPoolConfig {

    long getMaxIdleTime();

    int getScaleFactor();

    long getCheckInterval();

    interface Builder extends ThreadPoolConfig.Builder {

        @Override
        Builder setMinSize(int minSize);

        @Override
        Builder setMaxSize(int maxSize);

        @Override
        Builder setQueueCapacity(int queueCapacity);

        Builder setMaxIdleTime(long maxIdleTime);

        Builder setScaleFactor(int scaleFactor);

        Builder setCheckInterval(long checkInterval);

        @Override
        AutoScaleThreadPoolConfig build();
    }

}