using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using MyDotey.ObjectPool.AutoScale;
using MyDotey.ObjectPool.Facade;
using MyDotey.ObjectPool.ThreadPool;
using MyDotey.ObjectPool.ThreadPool.AutoScale;

/**
 * @author koqizhao
 *
 * Feb 25, 2018
 */
namespace MyDotey.ObjectPool.Tests
{
    public class AutoScaleObjectPoolTest : ObjectPoolTest
    {
        [ThreadStatic]
        protected static long _objectTtl = long.MaxValue;

        protected class TestAutoScaleThreadPool : AutoScaleThreadPool
        {
            public TestAutoScaleThreadPool(IAutoScaleThreadPoolConfig config)
                : base(config)
            {
            }

            protected override void SetObjectPoolConfigBuilder<B>(ObjectPool.IAbstractBuilder<WorkerThread, B> builder)
            {
                base.SetObjectPoolConfigBuilder(builder);
                ((ObjectPool.AutoScale.IBuilder<WorkerThread>)builder).SetObjectTtl(_objectTtl);
            }
        }

        protected override IThreadPool NewThreadPool()
        {
            return NewThreadPool(0);
        }

        protected override IThreadPool NewThreadPool(int queueCapacity)
        {
            return NewThreadPool(2 * 1000, 2 * 1000, 10 * 1000, queueCapacity);
        }

        protected virtual IThreadPool NewThreadPool(long checkInterval, long maxIdleTime, long ttl)
        {
            return NewThreadPool(checkInterval, maxIdleTime, ttl, 0);
        }

        protected virtual IThreadPool NewThreadPool(long checkInterval, long maxIdleTime, long ttl, int queueCapacity)
        {
            ThreadPool.AutoScale.IBuilder builder = ThreadPools.NewAutoScaleThreadPoolConfigBuilder();
            builder.SetMinSize(_minSize).SetMaxSize(_maxSize).SetScaleFactor(5).SetCheckInterval(checkInterval)
                    .SetMaxIdleTime(maxIdleTime).SetQueueCapacity(queueCapacity);
            _objectTtl = ttl;
            try
            {
                return new TestAutoScaleThreadPool(builder.Build());
            }
            finally
            {
                _objectTtl = long.MaxValue;
            }
        }

        [Fact]
        public override void ThreadPoolSubmitTaskTest4()
        {
            int taskCount = _maxSize;
            long taskSleep = 500;
            long viInitDelay = _defaultViInitDelay;
            int sizeAfterSubmit = _maxSize;
            long finishSleep = 5000;
            int finalSize = _minSize;
            ThreadPoolSubmitTaskTestInternal(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize);
        }

        [Fact]
        public override void ThreadPoolSubmitTaskTest6()
        {
            int taskCount = 200;
            long taskSleep = 2000;
            long viInitDelay = _defaultViInitDelay;
            int sizeAfterSubmit = _maxSize;
            long finishSleep = 10000;
            int finalSize = _minSize;
            ThreadPoolSubmitTaskTestInternal(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize);
        }

        [Fact]
        public override void ThreadPoolSubmitTaskTest7()
        {
            int taskCount = 200;
            long taskSleep = 2000;
            long viInitDelay = _defaultViInitDelay;
            int sizeAfterSubmit = _maxSize;
            long finishSleep = 10000;
            int finalSize = _minSize;
            int queueCapacity = 10;
            ThreadPoolSubmitTaskTestInternal(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize,
                NewThreadPool(queueCapacity));
        }


        [Fact]
        public virtual void ThreadPoolSubmitTaskMoreTest()
        {
            int taskCount = 200;
            long taskSleep = 2000;
            long viInitDelay = _defaultViInitDelay;
            int sizeAfterSubmit = _maxSize;
            long finishSleep = 20000;
            int finalSize = _maxSize;
            long checkInterval = 2000;
            long maxIdleTime = 10000;
            long ttl = 5000;
            IThreadPool pool = NewThreadPool(checkInterval, maxIdleTime, ttl);
            ThreadPoolSubmitTaskTestInternal(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize, pool);
        }

        [Fact]
        public virtual void ThreadPoolSubmitTaskMoreTest2()
        {
            int taskCount = 200;
            long taskSleep = 2000;
            long viInitDelay = _defaultViInitDelay;
            int sizeAfterSubmit = _maxSize;
            long finishSleep = 10000;
            int finalSize = _maxSize;
            long checkInterval = 2000;
            long maxIdleTime = long.MaxValue;
            long ttl = 10000;
            IThreadPool pool = NewThreadPool(checkInterval, maxIdleTime, ttl);
            ThreadPoolSubmitTaskTestInternal(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize, pool);
        }

        [Fact]
        public virtual void ThreadPoolSubmitTaskMoreTest3()
        {
            int taskCount = 200;
            long taskSleep = 2000;
            long viInitDelay = _defaultViInitDelay;
            int sizeAfterSubmit = _maxSize;
            long finishSleep = 20000;
            int finalSize = _minSize;
            long checkInterval = 2000;
            long maxIdleTime = 10000;
            long ttl = long.MaxValue;
            IThreadPool pool = NewThreadPool(checkInterval, maxIdleTime, ttl);
            ThreadPoolSubmitTaskTestInternal(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize, pool);
        }

        [Fact]
        public override void ThreadPoolSubmitTaskConcurrentTest()
        {
            int taskCount = 200;
            long taskSleep = 2000;
            long viInitDelay = _defaultViInitDelay;
            int sizeAfterSubmit = _defaultSizeAfterSubmit;
            long finishSleep = 10000;
            int finalSize = _minSize;
            ThreadPoolSubmitTaskTestInternal(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize,
                    NewThreadPool(), SubmissionMode.Concurrent);
        }

        [Fact]
        public override void ThreadPoolSubmitTaskConcurrentTest2()
        {
            int taskCount = 200;
            long taskSleep = 2000;
            long viInitDelay = _defaultViInitDelay;
            int sizeAfterSubmit = _defaultSizeAfterSubmit;
            long finishSleep = 10000;
            int finalSize = _minSize;
            ThreadPoolSubmitTaskTestInternal(taskCount, taskSleep, viInitDelay, sizeAfterSubmit, finishSleep, finalSize,
                    NewThreadPool(), SubmissionMode.SelfSelf);
        }
    }
}