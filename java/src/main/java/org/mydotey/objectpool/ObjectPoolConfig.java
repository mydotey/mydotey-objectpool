package org.mydotey.objectpool;

import java.util.function.Consumer;
import java.util.function.Supplier;

import org.mydotey.objectpool.ObjectPool.Entry;

/**
 * @author koqizhao
 *
 * Feb 6, 2018
 */
public interface ObjectPoolConfig<T> {

    int getMinSize();

    int getMaxSize();

    Supplier<T> getObjectFactory();

    Consumer<Entry<T>> getOnCreate();

    Consumer<Entry<T>> getOnClose();

    interface Builder<T> extends AbstractBuilder<T, Builder<T>> {

    }

    interface AbstractBuilder<T, B extends AbstractBuilder<T, B>> {

        B setMinSize(int minSize);

        B setMaxSize(int maxSize);

        B setObjectFactory(Supplier<T> objectFactory);

        B setOnCreate(Consumer<Entry<T>> onCreate);

        B setOnClose(Consumer<Entry<T>> onClose);

        ObjectPoolConfig<T> build();

    }

}