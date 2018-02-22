package org.mydotey.objectpool.autoscale;

import java.util.function.Function;

import org.mydotey.objectpool.ObjectPoolConfig;

/**
 * @author koqizhao
 *
 * Feb 6, 2018
 */
public interface AutoScaleObjectPoolConfig<T> extends ObjectPoolConfig<T> {

    long getObjectTtl();

    long getMaxIdleTime();

    Function<T, Boolean> getStaleChecker();

    long getCheckInterval();

    int getScaleFactor();

    interface Builder<T> extends AbstractBuilder<T, Builder<T>> {

    }

    interface AbstractBuilder<T, B extends AbstractBuilder<T, B>> extends ObjectPoolConfig.AbstractBuilder<T, B> {

        B setObjectTtl(long objectTtl);

        B setMaxIdleTime(long maxIdleTime);

        B setStaleChecker(Function<T, Boolean> staleChecker);

        B setCheckInterval(long checkInterval);

        B setScaleFactor(int scaleFactor);

        @Override
        AutoScaleObjectPoolConfig<T> build();
    }

}