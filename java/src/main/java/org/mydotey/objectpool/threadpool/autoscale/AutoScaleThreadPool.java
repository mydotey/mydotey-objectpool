package org.mydotey.objectpool.threadpool.autoscale;

import org.mydotey.objectpool.threadpool.ThreadPool;

/**
 * @author koqizhao
 *
 * Feb 24, 2018
 */
public interface AutoScaleThreadPool extends ThreadPool {

    @Override
    AutoScaleThreadPoolConfig getConfig();

}
