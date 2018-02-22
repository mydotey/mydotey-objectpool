package org.mydotey.objectpool.facade;

import org.mydotey.objectpool.DefaultObjectPool;
import org.mydotey.objectpool.DefaultObjectPoolConfig;
import org.mydotey.objectpool.ObjectPool;
import org.mydotey.objectpool.ObjectPoolConfig;
import org.mydotey.objectpool.autoscale.AutoScaleObjectPool;
import org.mydotey.objectpool.autoscale.AutoScaleObjectPoolConfig;
import org.mydotey.objectpool.autoscale.DefaultAutoScaleObjectPool;
import org.mydotey.objectpool.autoscale.DefaultAutoScaleObjectPoolConfig;

/**
 * @author koqizhao
 *
 * Feb 7, 2018
 */
public class ObjectPools {

    public static <T> ObjectPoolConfig.Builder<T> newObjectPoolConfigBuilder() {
        return new DefaultObjectPoolConfig.Builder<T>();
    }

    public static <T> ObjectPool<T> newObjectPool(ObjectPoolConfig<T> config) {
        return new DefaultObjectPool<>(config);
    }

    public static <T> AutoScaleObjectPoolConfig.Builder<T> newAutoScaleObjectPoolConfigBuilder() {
        return new DefaultAutoScaleObjectPoolConfig.Builder<T>();
    }

    public static <T> AutoScaleObjectPool<T> newAutoScaleObjectPool(AutoScaleObjectPoolConfig<T> config) {
        return new DefaultAutoScaleObjectPool<>(config);
    }

}
