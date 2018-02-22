package org.mydotey.objectpool.threadpool;

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

        Builder setMaxIdleTime(long maxIdleTime);

        Builder setScaleFactor(int scaleFactor);

        Builder setCheckInterval(long checkInterval);

        @Override
        AutoScaleThreadPoolConfig build();
    }

}