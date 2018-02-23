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
public class DefaultAutoScaleThreadPool extends DefaultThreadPool {

    public DefaultAutoScaleThreadPool(AutoScaleThreadPoolConfig.Builder builder) {
        super(builder);
    }

    @Override
    protected AutoScaleObjectPool<WorkerThread> newObjectPool(ThreadPoolConfig.Builder builder) {
        AutoScaleObjectPoolConfig<WorkerThread> config = ((DefaultAutoScaleThreadPoolConfig.Builder) builder)
                .setThreadPool(this).build();
        return ObjectPools.newAutoScaleObjectPool(config);
    }

    @Override
    protected ObjectPool<WorkerThread> getObjectPool() {
        return super.getObjectPool();
    }

}
