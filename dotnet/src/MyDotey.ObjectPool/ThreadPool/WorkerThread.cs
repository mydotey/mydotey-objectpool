using System;
using System.Threading;
using NLog;

/**
 * @author koqizhao
 *
 * Feb 22, 2018
 */
namespace MyDotey.ObjectPool.ThreadPool
{
    public class WorkerThread
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger();

        protected internal virtual Thread InnerThread { get; }

        private volatile Action _task;

        protected internal virtual IEntry<WorkerThread> PoolEntry { get; set; }

        private Action<WorkerThread> _onTaskComplete;

        private object _lock = new object();

        private volatile bool _isStarted;

        protected internal WorkerThread(Action<WorkerThread> onTaskComplete)
        {
            if (onTaskComplete == null)
                throw new ArgumentNullException("onTaskComplete is null");

            _onTaskComplete = onTaskComplete;

            InnerThread = new Thread(Run);
            InnerThread.IsBackground = true;
        }

        public virtual void Run()
        {
            while (true)
            {
                lock (_lock)
                {
                    if (!_isStarted)
                        _isStarted = true;

                    if (_task != null)
                    {
                        _task = null;

                        try
                        {
                            _onTaskComplete(this);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "onTaskComplete threw exception");
                            break;
                        }
                    }

                    try
                    {
                        Monitor.Wait(_lock);
                    }
                    catch
                    {
                        break;
                    }

                    try
                    {
                        _task();
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "task threw exception");
                    }
                }
            }

            _logger.Info("thread terminated: {0}", InnerThread);
        }

        protected internal virtual void SetTask(Action task)
        {
            lock (_lock)
            {
                _task = task;
                Monitor.Pulse(_lock);
            }
        }

        protected internal virtual void Start()
        {
            InnerThread.Start();

            while (!_isStarted)
            {
                try
                {
                    Thread.Sleep(1);
                }
                catch (ThreadInterruptedException )
                {
                    _logger.Info("thread {0} interrupted when starting", InnerThread);
                    InnerThread.Interrupt();
                    break;
                }
            }
        }
    }
}