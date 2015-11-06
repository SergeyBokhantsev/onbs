using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HostController
{
    public class HostTimer : IHostTimer
    {
        private readonly EventWaitHandle shedulerSignal;
        private readonly Action<IHostTimer> action;

        private int span;
        private bool enabled;

        public bool Disposed
        {
            get;
            private set;
        }

        public int Span
        {
            get
            {
                return span;
            }
            set
            {
                if (span > 0)
                {
                    span = value;
                    shedulerSignal.Set();
                }
                else
                    throw new ArgumentException("Span must be positive!");
            }
        }

        public bool IsEnabled
        {
            get
            {
                return Disposed ? false : enabled;
            }
            set
            {
                enabled = value;
                shedulerSignal.Set();
            }
        }

        public HostTimer(EventWaitHandle shedulerSignal, int span, Action<IHostTimer> action, bool isEnabled)
        {
            if (shedulerSignal == null)
                throw new ArgumentNullException("schedulerSignal");

            if (span <= 0)
                throw new ArgumentException("Span must be positive!");

            this.shedulerSignal = shedulerSignal;
            this.span = span;
            this.action = action;
            this.enabled = isEnabled;
        }

        public void Execute()
        {
            if (!Disposed && action != null)
                action(this);
        }

        public void Dispose()
        {
            Disposed = true;
        }
    }

    internal class HostTimersController : IDisposable
    {
        private class TimerInfo
        {
            public HostTimer Timer;
            public DateTime LastExecutionTime;
        }

        private Thread schedulerThread;
        private readonly List<TimerInfo> timers = new List<TimerInfo>();
        private readonly SynchronizationContext syncContext;
        private readonly AutoResetEvent schedulerSignal = new AutoResetEvent(false);

        private bool disposed;

        public HostTimersController(SynchronizationContext syncContext)
        {
            if (syncContext == null)
                throw new ArgumentNullException("syncContext");

            this.syncContext = syncContext;
        }

        public void Start()
        {
            if (schedulerThread != null)
                throw new InvalidOperationException("already started");

            schedulerThread = new Thread(Scheduler);
            schedulerThread.IsBackground = true;
            schedulerThread.Start();
        }

        public HostTimer CreateTimer(int span, Action<IHostTimer> action, bool isEnabled, bool firstEventImmidiatelly)
        {
            var timer = new HostTimer(schedulerSignal, span, action, isEnabled);

            lock(timers)
            {
                timers.Add(new TimerInfo { Timer = timer, LastExecutionTime = firstEventImmidiatelly ? DateTime.MinValue : DateTime.Now });
            }

            schedulerSignal.Set();
            return timer;
        }

        private void Scheduler()
        {
            int waitTime = -1;

            while (!disposed)
            {
                if (waitTime != 0)
                    schedulerSignal.WaitOne(waitTime);

                var now = DateTime.Now;

                lock (timers)
                {
                    timers.RemoveAll(t => t.Timer.Disposed);

                    waitTime = -1; 

                    foreach (var info in timers)
                    {
                        if (info.Timer.IsEnabled)
                        {
                            int nextExecutionSpan = 0;

                            if (info.LastExecutionTime.AddMilliseconds(info.Timer.Span) <= now)
                            {
                                syncContext.Post(PostTimerExecution, info.Timer);
                                info.LastExecutionTime = now;
                                nextExecutionSpan = info.Timer.Span;
                            }
                            else
                            {
                                nextExecutionSpan = Math.Max(0, info.Timer.Span - (int)(now - info.LastExecutionTime).TotalMilliseconds);
                            }

                            if (waitTime == -1 || waitTime > nextExecutionSpan)
                                waitTime = nextExecutionSpan;
                        }
                    }
                }
            }
        }

        private void PostTimerExecution(object state)
        {
            var timer = state as HostTimer;

            if (timer != null && timer.IsEnabled)
                timer.Execute();
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
            }
        }
    }
}
