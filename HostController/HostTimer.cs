using Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;

namespace HostController
{
    public class HostTimer : IHostTimer
    {
        private readonly EventWaitHandle schedulerSignal;
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
                    schedulerSignal.Set();
                }
                else
                    throw new ArgumentException("Span must be positive!");
            }
        }

        public bool IsEnabled
        {
            get
            {
                return !Disposed && enabled;
            }
            set
            {
                enabled = value;
                schedulerSignal.Set();
            }
        }

        public HostTimer(EventWaitHandle schedulerSignal, int span, Action<IHostTimer> action, bool isEnabled)
        {
            if (schedulerSignal == null)
                throw new ArgumentNullException("schedulerSignal");

            if (span <= 0)
                throw new ArgumentException("Span must be positive!");

            this.schedulerSignal = schedulerSignal;
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
            public int LastExecutionTime;
            public string Details;
        }

        private Thread schedulerThread;
        private readonly List<TimerInfo> timers = new List<TimerInfo>();
        private readonly ONBSSyncContext syncContext;
        private readonly AutoResetEvent schedulerSignal = new AutoResetEvent(false);

        private bool disposed;

        public HostTimersController(ONBSSyncContext syncContext)
        {
            if (syncContext == null)
                throw new ArgumentNullException("syncContext");

            this.syncContext = syncContext;
        }

        public void Start()
        {
            if (schedulerThread != null)
                throw new InvalidOperationException("already started");

            schedulerThread = new Thread(Scheduler) 
			{
				IsBackground = true,
				Name = "TimerScheduler"
			};
            schedulerThread.Start();
        }

        public HostTimer CreateTimer(int span, Action<IHostTimer> action, bool isEnabled, bool firstEventImmidiatelly, string details)
        {
            var timer = new HostTimer(schedulerSignal, span, action, isEnabled);

            lock(timers)
            {
                timers.Add(new TimerInfo { Timer = timer, 
                                           LastExecutionTime = firstEventImmidiatelly ? int.MinValue : Environment.TickCount,
                                           Details = details });
            }

            schedulerSignal.Set();
            return timer;
        }

        private void Scheduler()
        {
            int waitTime = -1;

            while (!disposed)
            {
                if (waitTime > 0)
                    schedulerSignal.WaitOne(waitTime);

                var now = Environment.TickCount;

                lock (timers)
                {
                    timers.RemoveAll(t => t.Timer.Disposed);

                    waitTime = -1; 

                    foreach (var info in timers)
                    {
                        if (info.Timer.IsEnabled)
                        {
                            int nextExecutionSpan;

                            if (info.LastExecutionTime + info.Timer.Span <= now)
                            {
                                syncContext.Post(PostTimerExecution, info.Timer, info.Details);
                                info.LastExecutionTime = now;
                                nextExecutionSpan = info.Timer.Span;
                            }
                            else
                            {
                                nextExecutionSpan = Math.Max(0, info.Timer.Span - (now - info.LastExecutionTime));
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
