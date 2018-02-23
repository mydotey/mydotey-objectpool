using System;
using System.Threading;

/**
 * @author koqizhao
 *
 * Feb 22, 2018
 */
namespace MyDotey.ObjectPool.ThreadPool
{
    public class WorkerThread
    {
        //private static Logger _logger = LoggerFactory.getLogger(WorkerThread.class);

        private Thread _thread;

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

            _thread = new Thread(Run);
            _thread.IsBackground = true;
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
                            //_logger.error("onTaskComplete threw exception", ex);
                            break;
                        }
                    }

                    try
                    {
                        Monitor.Wait(_lock);
                    }
                    catch (ThreadInterruptedException e)
                    {
                        break;
                    }

                    try
                    {
                        _task();
                    }
                    catch (Exception e)
                    {
                        //_logger.error("task threw exception", e);
                    }
                }
            }

            //_logger.info("thread terminated: {}", _thread);
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
            _thread.Start();

            while (!_isStarted)
            {
                try
                {
                    Thread.Sleep(1);
                }
                catch (ThreadInterruptedException e)
                {
                    //_logger.info("thread {} interrupted when starting", this);
                    break;
                }
            }
        }

        protected internal virtual void Interrupt() {
            _thread.Interrupt();
        }
    }
}