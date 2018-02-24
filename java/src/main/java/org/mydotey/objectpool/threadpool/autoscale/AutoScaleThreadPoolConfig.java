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

    interface Builder extends AbstractBuilder<Builder> {

    }

    interface AbstractBuilder<B extends AbstractBuilder<B>> extends ThreadPoolConfig.AbstractBuilder<B> {

        B setMaxIdleTime(long maxIdleTime);

        B setScaleFactor(int scaleFactor);

        B setCheckInterval(long checkInterval);

        @Override
        AutoScaleThreadPoolConfig build();
    }

}