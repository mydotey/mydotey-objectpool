package org.mydotey.objectpool.autoscale;

import org.mydotey.objectpool.ObjectPool;

/**
 * @author koqizhao
 *
 * Feb 6, 2018
 */
public interface AutoScaleObjectPool<T> extends ObjectPool<T> {

    @Override
    AutoScaleObjectPoolConfig<T> getConfig();

}