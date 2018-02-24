package org.mydotey.objectpool.threadpool.autoscale;

import org.mydotey.objectpool.ObjectPool;
import org.mydotey.objectpool.autoscale.AutoScaleObjectPool;
import org.mydotey.objectpool.autoscale.AutoScaleObjectPoolConfig;
import org.mydotey.objectpool.facade.ObjectPools;
import org.mydotey.objectpool.threadpool.DefaultThreadPool;
import org.mydotey.objectpool.threadpool.ThreadPoolConfig;
import org.mydotey.objectpool.threadpool.WorkerThread;
import org.mydotey.objectpool.threadpool.autoscale.AutoScaleThreadPoolConfig;
import org.mydotey.objectpool.threadpool.autoscale.DefaultAutoScaleThreadPoolConfig;

/**
 * @author koqizhao
 *
 *         Feb 6, 2018
 */
public class DefaultAutoScaleThreadPool extends DefaultThreadPool implements AutoScaleThreadPool {

    public DefaultAutoScaleThreadPool(AutoScaleThreadPoolConfig.Builder builder) {
        super(builder);
    }

    @Override
    public AutoScaleThreadPoolConfig getConfig() {
        return (AutoScaleThreadPoolConfig) super.getConfig();
    }

    @Override
    protected ThreadPoolConfig newConfig(ThreadPoolConfig.Builder builder) {
        return ((DefaultAutoScaleThreadPoolConfig.Builder) builder).setThreadPool(this).build();
    }

    @SuppressWarnings("unchecked")
    @Override
    protected AutoScaleObjectPool<WorkerThread> newObjectPool() {
        return ObjectPools.newAutoScaleObjectPool((AutoScaleObjectPoolConfig<WorkerThread>) getConfig());
    }

    @Override
    protected ObjectPool<WorkerThread> getObjectPool() {
        return super.getObjectPool();
    }

}
