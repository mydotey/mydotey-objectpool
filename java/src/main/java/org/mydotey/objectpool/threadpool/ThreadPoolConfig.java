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

    interface Builder extends AbstractBuilder<Builder> {

    }

    interface AbstractBuilder<B extends AbstractBuilder<B>> {

        B setMinSize(int minSize);

        B setMaxSize(int maxSize);

        B setQueueCapacity(int queueCapacity);

        ThreadPoolConfig build();
    }

}