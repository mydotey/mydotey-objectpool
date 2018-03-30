# MyDotey ObjectPool dotnet

## NuGet package
```
dotnet add package MyDotey.ObjectPool -v 1.0.1
```

## ObjectPool
```cs
IBuilder<object> builder = ObjectPools.NewObjectPoolConfigBuilder<object>();
builder.SetMaxSize(10))
    .SetMinSize(1)
    .SetObjectFactory(() => new object());
IObjectPoolConfig<object> config = builder.Build();
IObjectPool<object> objectPool = ObjectPools.NewObjectPool(config);

IEntry<object> entry = null;
try
{
    entry = objectPool.Acquire();
    object o = entry.Object;
    // use object
}
finally
{
    if (entry != null)
        objectPool.Release(entry);
}
```

## AutoScaleObjectPool
```cs
IBuilder<object> builder = ObjectPools.NewAutoScaleObjectPoolConfigBuilder<object>();
builder.SetMaxSize(100))
    .SetMinSize(10)
    .SetObjectFactory(() => new object())
    .SetCheckInterval(5 * 1000))
    .SetObjectTtl(5 * 60 * 1000)
    .SetMaxIdleTime(1 * 60 * 1000)
    .SetScaleFactor(5)
    .SetStaleChecker(o => false);
IAutoScaleObjectPoolConfig<object> config = builder.Build();
IAutoScaleObjectPool<object> objectPool = ObjectPools.NewAutoScaleObjectPool(config);

IEntry<object> entry = null;
try
{
    entry = objectPool.Acquire();
    object o = entry.Object;
    // use object
}
finally
{
    if (entry != null)
        objectPool.Release(entry);
}
```

## ThreadPool
```cs
IBuilder builder = ThreadPools.NewThreadPoolConfigBuilder();
builder.SetMinSize(1)
    .SetMaxSize(10)
    .SetQueueCapacity(100);
IThreadPoolConfig config = builder.Build();
IThreadPool threadPool = ThreadPools.NewThreadPool(config);

threadPool.Submit(() => Console.WriteLine("Hello, world!"));
```

## AutoScaleThreadPool
```cs
IBuilder builder = ThreadPools.NewAutoScaleThreadPoolConfigBuilder();
builder.SetMinSize(10)
    .SetMaxSize(100)
    .SetScaleFactor(5)
    .SetCheckInterval(5 * 1000)
    .SetMaxIdleTime(1 * 60 * 1000)
    .SetQueueCapacity(1000);
IAutoScaleThreadPoolConfig config = builder.Build();
IAutoScaleThreadPool threadPool = ThreadPools.NewAutoScaleThreadPool(config);

threadPool.Submit(() => Console.WriteLine("Hello, world!"));
```
