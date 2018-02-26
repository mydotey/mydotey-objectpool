using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using NLog;
using NLog.Config;
using NLog.Targets;
using MyDotey.ObjectPool.Facade;
using MyDotey.ObjectPool.ThreadPool;

/**
 * @author koqizhao
 *
 * Feb 25, 2018
 */
namespace MyDotey.ObjectPool.Tests
{
    public class ObjectPoolTest
    {
        protected internal static long CurrentTimeMillis { get { return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond; } }

        protected int _minSize = 10;
        protected int _maxSize = 100;
        protected int _submitterThreadCount = 10;

        protected int _defaultTaskCount = 10;
        protected long _defaultTaskSleep = 0;
        protected long _defaultViInitDelay = 1000;
        protected int _defaultSizeAfterSubmit = -1;
        protected long _defaultFinishSleep = 0;
        protected int _defaultFinalSize = -1;

        public ObjectPoolTest()
        {
            var config = new LoggingConfiguration();
            var logconsole = new ConsoleTarget() { Name = "logconsole" };
            config.LoggingRules.Add(new NLog.Config.LoggingRule("*", LogLevel.Trace, logconsole));
            NLog.LogManager.Configuration = config;
        }

        protected virtual IThreadPool NewThreadPool()
        {
            return NewThreadPool(0);
        }

        protected virtual IThreadPool NewThreadPool(int queueCapacity)
        {
            IBuilder Builder = ThreadPools.NewThreadPoolConfigBuilder();
            Builder.SetMinSize(_minSize).SetMaxSize(_maxSize).SetQueueCapacity(queueCapacity);
            return ThreadPools.NewThreadPool(Builder.Build());
        }

        [Fact]
        public virtual void ThreadPoolCreateTest()
        {
            Console.WriteLine();
            Console.WriteLine();

            IThreadPool pool = NewThreadPool();
            Console.WriteLine("Pool Size: " + pool.Size);
            Assert.Equal(_minSize, pool.Size);
            pool.Dispose();
        }

        [Fact]
        public virtual void ThreadPoolSubmitTaskTest()
        {
            int taskCount = _minSize;
            long taskSleep = _defaultTaskSleep;
            long viInitDelay = _defaultViInitDelay;
            int sizeAfterSubmit = _minSize;
            long finishSleep = 1;
            int finalSize = _defaultFinalSize;
            ThreadPoolSubmitTaskTestInternal(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize);
        }

        [Fact]
        public virtual void ThreadPoolSubmitTaskTest2()
        {
            int taskCount = _minSize;
            long taskSleep = 10;
            long viInitDelay = _defaultViInitDelay;
            int sizeAfterSubmit = _minSize;
            long finishSleep = 1000;
            int finalSize = _minSize;
            ThreadPoolSubmitTaskTestInternal(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize);
        }

        [Fact]
        public virtual void ThreadPoolSubmitTaskTest3()
        {
            int taskCount = 50;
            long taskSleep = 100;
            long viInitDelay = _defaultViInitDelay;
            int sizeAfterSubmit = _defaultSizeAfterSubmit;
            long finishSleep = 1000;
            int finalSize = _defaultFinalSize;
            ThreadPoolSubmitTaskTestInternal(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize);
        }

        [Fact]
        public virtual void ThreadPoolSubmitTaskTest4()
        {
            int taskCount = _maxSize;
            long taskSleep = 500;
            long viInitDelay = _defaultViInitDelay;
            int sizeAfterSubmit = _maxSize;
            long finishSleep = 5000;
            int finalSize = _maxSize;
            ThreadPoolSubmitTaskTestInternal(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize);
        }

        [Fact]
        public virtual void ThreadPoolSubmitTaskTest5()
        {
            int taskCount = 200;
            long taskSleep = 100;
            long viInitDelay = _defaultViInitDelay;
            int sizeAfterSubmit = _maxSize;
            long finishSleep = 1000;
            int finalSize = _maxSize;
            ThreadPoolSubmitTaskTestInternal(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize);
        }

        [Fact]
        public virtual void ThreadPoolSubmitTaskTest6()
        {
            int taskCount = 200;
            long taskSleep = 2000;
            long viInitDelay = _defaultViInitDelay;
            int sizeAfterSubmit = _maxSize;
            long finishSleep = 5000;
            int finalSize = _maxSize;
            ThreadPoolSubmitTaskTestInternal(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize);
        }

        [Fact]
        public virtual void ThreadPoolSubmitTaskTest7()
        {
            int taskCount = 200;
            long taskSleep = 2000;
            long viInitDelay = _defaultViInitDelay;
            int sizeAfterSubmit = _maxSize;
            long finishSleep = 5000;
            int finalSize = _maxSize;
            int queueCapacity = 10;
            ThreadPoolSubmitTaskTestInternal(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize,
                NewThreadPool(queueCapacity));
        }

        [Fact(Skip = "stress")]
        public virtual void ThreadPoolSubmitTaskStressTest()
        {
            int count = 100;
            for (int i = 0; i < count; i++)
                ThreadPoolSubmitTaskTest();
        }

        [Fact(Skip = "stress")]
        public virtual void ThreadPoolSubmitTaskStressTest2()
        {
            int count = 100;
            for (int i = 0; i < count; i++)
                ThreadPoolSubmitTaskTest4();
        }

        [Fact(Skip = "stress")]
        public virtual void ThreadPoolSubmitTaskStressTest3()
        {
            int count = 100;
            for (int i = 0; i < count; i++)
                ThreadPoolSubmitTaskTest7();
        }

        [Fact]
        public virtual void ThreadPoolSubmitTaskConcurrentTest()
        {
            int taskCount = 200;
            long taskSleep = 2000;
            long viInitDelay = _defaultViInitDelay;
            int sizeAfterSubmit = _defaultSizeAfterSubmit;
            long finishSleep = 5000;
            int finalSize = _maxSize;
            ThreadPoolSubmitTaskTestInternal(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize,
                    NewThreadPool(), SubmissionMode.Concurrent);
        }

        [Fact]
        public virtual void ThreadPoolSubmitTaskConcurrentTest2()
        {
            int taskCount = 200;
            long taskSleep = 2000;
            long viInitDelay = _defaultViInitDelay;
            int sizeAfterSubmit = _defaultSizeAfterSubmit;
            long finishSleep = 5000;
            int finalSize = _maxSize;
            ThreadPoolSubmitTaskTestInternal(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize,
                    NewThreadPool(), SubmissionMode.SelfSelf);
        }

        protected virtual void ThreadPoolSubmitTaskTestInternal(int taskCount, long taskSleep, long viInitDelay, int sizeAfterSubmit,
                long finishSleep, int finalSize)
        {
            ThreadPoolSubmitTaskTestInternal(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize,
                    NewThreadPool());
        }

        protected virtual void ThreadPoolSubmitTaskTestInternal(int taskCount, long taskSleep, long viInitDelay, int sizeAfterSubmit,
                long finishSleep, int finalSize, IThreadPool customPool)
        {
            ThreadPoolSubmitTaskTestInternal(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize, customPool,
                    SubmissionMode.SingleThread);
        }

        protected virtual void ThreadPoolSubmitTaskTestInternal(int taskCount, long taskSleep, long viInitDelay, int sizeAfterSubmit,
                long finishSleep, int finalSize, IThreadPool customPool, SubmissionMode submissionMode)
        {
            Console.WriteLine();
            Console.WriteLine();

            int counter = 0;
            CountdownEvent countDownLatch = new CountdownEvent(taskCount);
            Action task = () =>
            {
                Interlocked.Increment(ref counter);
                countDownLatch.Signal();
                if (taskSleep <= 0)
                    return;

                try
                {
                    Thread.Sleep((int)taskSleep);
                }
                catch
                {
                }
            };
            long now = CurrentTimeMillis;
            Timer ViTimer = null;
            IThreadPool self = null;
            using (IThreadPool pool = customPool)
            {
                Console.WriteLine("new thread pool eclipsed: " + (CurrentTimeMillis - now));
                Console.WriteLine("counter value: " + counter);
                Vi(pool);
                Assert.Equal(0, counter);

                using (ViTimer = new Timer(s =>
                 {
                     Console.WriteLine("counter value: " + counter);
                     Vi(pool);
                 }, null, 1000, 500))
                {
                    switch (submissionMode)
                    {
                        case SubmissionMode.SingleThread:
                            for (int i = 0; i < taskCount; i++)
                                pool.Submit(task);
                            break;
                        case SubmissionMode.Concurrent:
                            now = CurrentTimeMillis;
                            for (int i = 0; i < taskCount; i++)
                                Task.Run(() =>
                                {
                                    try
                                    {
                                        pool.Submit(task);
                                    }
                                    catch (ThreadInterruptedException e)
                                    {
                                        Console.WriteLine(e);
                                    }
                                });
                            break;
                        case SubmissionMode.SelfSelf:
                            self = NewThreadPool();
                            for (int i = 0; i < taskCount; i++)
                                self.Submit(() =>
                                {
                                    try
                                    {
                                        pool.Submit(task);
                                    }
                                    catch (ThreadInterruptedException e)
                                    {
                                        Console.WriteLine(e);
                                    }
                                });
                            break;
                        default:
                            throw new Exception("impossible submissionMode: " + submissionMode);
                    }

                    Console.WriteLine("Submit tasks eclipsed: " + (CurrentTimeMillis - now));
                    Console.WriteLine("counter value: " + counter);
                    Vi(pool);

                    Assert.True(_minSize <= pool.Size);
                    Assert.True(_maxSize >= pool.Size);
                    if (sizeAfterSubmit >= 0)
                        Assert.Equal(sizeAfterSubmit, pool.Size);

                    countDownLatch.Wait();

                    Console.WriteLine("tasks run time: " + (CurrentTimeMillis - now));
                    Console.WriteLine("counter value: " + counter);
                    Vi(pool);

                    Assert.Equal(taskCount, counter);
                    Assert.True(_minSize <= pool.Size);
                    Assert.True(_maxSize >= pool.Size);

                    if (finishSleep <= 0)
                        return;

                    Thread.Sleep((int)finishSleep);

                    Vi(pool);

                    Assert.Equal(taskCount, counter);
                    Assert.True(_minSize <= pool.Size);
                    Assert.True(_maxSize >= pool.Size);

                    if (finalSize >= 0)
                        Assert.Equal(finalSize, pool.Size);
                }
            }
        }

        protected virtual void Vi(IThreadPool pool)
        {
            Console.WriteLine();
            IObjectPool<WorkerThread> objectPool = ((DefaultThreadPool)pool).ObjectPool;
            Console.WriteLine("pool size: " + objectPool.Size);
            Console.WriteLine("pool acquired size: " + objectPool.AcquiredSize);
            Console.WriteLine("pool available size: " + objectPool.AvailableSize);
            Console.WriteLine();
        }

        protected enum SubmissionMode
        {
            SingleThread, Concurrent, SelfSelf
        }
    }
}
