package org.mydotey.objectpool.threadpool.autoscale;

import org.mydotey.objectpool.ObjectPool;
import org.mydotey.objectpool.autoscale.AutoScaleObjectPool;
import org.mydotey.objectpool.autoscale.AutoScaleObjectPoolConfig;
import org.mydotey.objectpool.facade.ObjectPools;
import org.mydotey.objectpool.threadpool.DefaultThreadPool;
import org.mydotey.objectpool.threadpool.WorkerThread;
import org.mydotey.objectpool.threadpool.autoscale.AutoScaleThreadPoolConfig;

/**
 * @author koqizhao
 *
 *         Feb 6, 2018
 */
public class DefaultAutoScaleThreadPool extends DefaultThreadPool implements AutoScaleThreadPool {

    public DefaultAutoScaleThreadPool(AutoScaleThreadPoolConfig config) {
        super(config);
    }

    @Override
    public AutoScaleThreadPoolConfig getConfig() {
        return (AutoScaleThreadPoolConfig) super.getConfig();
    }

    @Override
    protected AutoScaleObjectPool<WorkerThread> newObjectPool() {
        AutoScaleObjectPoolConfig.Builder<WorkerThread> builder = ObjectPools.newAutoScaleObjectPoolConfigBuilder();
        builder.setCheckInterval(getConfig().getCheckInterval()).setMaxIdleTime(getConfig().getMaxIdleTime())
                .setScaleFactor(getConfig().getScaleFactor())
                .setStaleChecker(t -> t.getState() == Thread.State.TERMINATED);
        setObjectPoolConfigBuilder(builder);
        return ObjectPools.newAutoScaleObjectPool(builder.build());
    }

    @Override
    protected ObjectPool<WorkerThread> getObjectPool() {
        return super.getObjectPool();
    }

}
