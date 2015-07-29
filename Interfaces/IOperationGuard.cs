using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IOperationGuard : IDisposable
    {
        bool ExecuteIfFree(Action action, Action<Exception> exceptionHandler = null);
    }

    public class InterlockedGuard : IOperationGuard
    {
        private int busy;
        private bool disposed;

        public bool ExecuteIfFree(Action action, Action<Exception> exceptionHandler = null)
        {
            if (!disposed && Interlocked.Exchange(ref busy, 1) == 0)
            {
                try
                {
                    action();
                }
                catch(Exception ex)
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

        private DateTime lastExecutionTime;
        private bool disposed;

        public TimedGuard(TimeSpan minInterval)
        {
            this.minInterval = minInterval;
        }

        public bool ExecuteIfFree(Action action, Action<Exception> exceptionHandler = null)
        {
            if (!disposed && (DateTime.Now - lastExecutionTime) >= minInterval)
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
