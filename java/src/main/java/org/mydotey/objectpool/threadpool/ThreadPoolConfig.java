package org.mydotey.objectpool.threadpool;

/**
 * @author koqizhao
 *
 * Feb 6, 2018
 */
public interface ThreadPoolConfig {

    int getMinSize();

    int getMaxSize();

    int getQueueCapacity();

    interface Builder {

        Builder setMinSize(int minSize);

        Builder setMaxSize(int maxSize);

        Builder setQueueCapacity(int queueCapacity);

        ThreadPoolConfig build();
    }

}