using System;
using System.Threading;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IOperationGuard : IDisposable
    {
        bool ExecuteIfFree(Action action, Action<Exception> exceptionHandler = null);
        Task<bool> ExecuteIfFreeAsync(Func<Task> taskAccessor, Action<Exception> exceptionHandler = null);
        bool ExecuteIfFreeDedicatedThread(Action action, Action<Exception> exceptionHandler = null);
    }

    public class InterlockedGuard : IOperationGuard
    {
        private int busy;
        private bool disposed;

        public bool ExecuteIfFreeDedicatedThread(Action action, Action<Exception> exceptionHandler = null)
        {
            if (!disposed && Interlocked.Exchange(ref busy, 1) == 0)
            {
                try
                {
                    return ThreadPool.QueueUserWorkItem((state) =>
                    {
                        try
                        {
                            action();
                        }
                        catch (Exception ex)
                        {
                            if (exceptionHandler != null)
                                exceptionHandler(ex);
                        }
                        finally
                        {
                            Interlocked.Exchange(ref busy, 0);
                        }
                    });
                }
                catch (Exception ex)
                {
                    Interlocked.Exchange(ref busy, 0);

                    if (exceptionHandler != null)
                        exceptionHandler(ex);

                    return false;
                }
            }
            else
                return false;
        }

        public async Task<bool> ExecuteIfFreeAsync(Func<Task> taskAccessor, Action<Exception> exceptionHandler = null)
        {
            if (!disposed && Interlocked.Exchange(ref busy, 1) == 0)
            {
                try
                {
                    await taskAccessor();
                }
                catch (AggregateException ex)
                {
                    if (exceptionHandler != null)
                    {
                        var inner = ex.Flatten();
                        exceptionHandler(inner);
                    }
                }
                catch (Exception ex)
                {
                    if (exceptionHandler != null)
                        exceptionHandler(ex);
                }
                finally
                {
                    Interlocked.Exchange(ref busy, 0);
                }

                return true;
            }
            else
                return false;
        }

        public bool ExecuteIfFree(Action action, Action<Exception> exceptionHandler = null)
        {
            if (!disposed && Interlocked.Exchange(ref busy, 1) == 0)
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    if (exceptionHandler != null)
                        exceptionHandler(ex);
                }
                finally
                {
                    Interlocked.Exchange(ref busy, 0);
                }

                return true;
            }
            else
                return false;
        }

        public void Dispose()
        {
            disposed = true;
        }
    }

    public class TimedGuard : IOperationGuard
    {
        private readonly TimeSpan minInterval;
        private readonly object locker = new object();

        private DateTime lastExecutionTime;
        private bool disposed;

        public TimedGuard(TimeSpan minInterval)
        {
            this.minInterval = minInterval;
        }

        public bool ExecuteIfFreeDedicatedThread(Action action, Action<Exception> exceptionHandler = null)
        {
            lock (locker)
            {
                if (!disposed && DateTime.Now - lastExecutionTime >= minInterval)
                {
                    lastExecutionTime = DateTime.Now;
                }
                else
                    return false;
            }

            try
            {
                return ThreadPool.QueueUserWorkItem((state) =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        if (exceptionHandler != null)
                            exceptionHandler(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                if (exceptionHandler != null)
                    exceptionHandler(ex);

                return false;
            }
        }

        public async Task<bool> ExecuteIfFreeAsync(Func<Task> taskAccessor, Action<Exception> exceptionHandler = null)
        {
            lock (locker)
            {
                if (!disposed && DateTime.Now - lastExecutionTime >= minInterval)
                {
                    lastExecutionTime = DateTime.Now;
                }
                else
                    return false;
            }

            try
            {
                await taskAccessor();
            }
            catch (AggregateException ex)
            {
                if (exceptionHandler != null)
                {
                    var inner = ex.Flatten();
                    exceptionHandler(inner);
                }
            }
            catch (Exception ex)
            {
                if (exceptionHandler != null)
                    exceptionHandler(ex);
            }

            return true;
        }

        public bool ExecuteIfFree(Action action, Action<Exception> exceptionHandler = null)
        {
            if (!disposed && DateTime.Now - lastExecutionTime >= minInterval)
            {
                try
                {
                    lastExecutionTime = DateTime.Now;
                    action();
                }
                catch (Exception ex)
                {
                    if (exceptionHandler != null)
                        exceptionHandler(ex);
                }

                return true;
            }
            else
                return false;
        }

        public void Dispose()
        {
            disposed = true;
        }
    }

    public class ManualResetGuard : IOperationGuard
    {
        private int busy;
        private bool disposed;

        public void Reset()
        {
            Interlocked.Exchange(ref busy, 0);
        }

        public bool ExecuteIfFreeDedicatedThread(Action action, Action<Exception> exceptionHandler = null)
        {
            if (!disposed && Interlocked.Exchange(ref busy, 1) == 0)
            {
                try
                {
                    return ThreadPool.QueueUserWorkItem((state) =>
                    {
                        try
                        {
                            action();
                        }
                        catch (Exception ex)
                        {
                            if (exceptionHandler != null)
                                exceptionHandler(ex);
                        }
                    });
                }
                catch (Exception ex)
                {
                    if (exceptionHandler != null)
                        exceptionHandler(ex);

                    return false;
                }
            }
            else
                return false;
        }

        public async Task<bool> ExecuteIfFreeAsync(Func<Task> taskAccessor, Action<Exception> exceptionHandler = null)
        {
            if (!disposed && Interlocked.Exchange(ref busy, 1) == 0)
            {
                try
                {
                    await taskAccessor();
                }
                catch (AggregateException ex)
                {
                    if (exceptionHandler != null)
                    {
                        var inner = ex.Flatten();
                        exceptionHandler(inner);
                    }
                }
                catch (Exception ex)
                {
                    if (exceptionHandler != null)
                        exceptionHandler(ex);

                    Reset();
                }

                return true;
            }
            else
                return false;
        }

        public bool ExecuteIfFree(Action action, Action<Exception> exceptionHandler = null)
        {
            if (!disposed && Interlocked.Exchange(ref busy, 1) == 0)
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    if (exceptionHandler != null)
                        exceptionHandler(ex);

                    Reset();
                }

                return true;
            }
            else
                return false;
        }

        public void Dispose()
        {
            disposed = true;
        }
    }
}
