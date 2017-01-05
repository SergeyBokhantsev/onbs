using Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;

namespace HostController
{
    public class HostTimer : IHostTimer
    {
        private readonly Action<IHostTimer> action;

        private bool enabled;

        private int iterations;
        private int remainingIterations;

        public bool Disposed
        {
            get;
            private set;
        }

        public int Span
        {
            get
            {
                return iterations * HostTimersController.TIMER_RESOLUTION;
            }
            set
            {
                if (value > 0)
                {
                    remainingIterations = iterations = (int)Math.Ceiling((double)value / (double)HostTimersController.TIMER_RESOLUTION);
                }
                else
                    throw new ArgumentException("Span must be positive!");
            }
        }

        public string Details
        {
            get;
            set;
        }

        public bool Decrement()
        {
            remainingIterations--;

            var result = remainingIterations < 0;

            if (result)
                remainingIterations = iterations;

            return result;
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
            }
        }

        public HostTimer(int span, Action<IHostTimer> action, bool isEnabled, bool firstEventImmidiatelly)
        {
            Span = span;

            if (firstEventImmidiatelly)
                remainingIterations = 0;

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
        public const int TIMER_RESOLUTION = 100;

        private Thread schedulerThread;
        private readonly List<HostTimer> timers = new List<HostTimer>();
        private readonly ONBSSyncContext syncContext;
        private readonly Configuration config;

        private bool disposed;

        public HostTimersController(ONBSSyncContext syncContext, Configuration config)
        {
            if (syncContext == null)
                throw new ArgumentNullException("syncContext");

            if (config == null)
                throw new ArgumentNullException("config");

            this.syncContext = syncContext;
            this.config = config;
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
            var timer = new HostTimer(span, action, isEnabled, firstEventImmidiatelly);

            timer.Details = details;

            lock(timers)
            {
                timers.Add(timer);
            }

            return timer;
        }

        private void Scheduler()
        {
            while (!disposed)
            {
                bool cleanupTimers = false;

                lock (timers)
                {
                    foreach(var timer in timers)
                    {
                        if (!timer.Disposed)
                        {
                            if (timer.IsEnabled && timer.Decrement())
                            {
                                syncContext.Post(PostTimerExecution, timer, timer.Details);
                            }
                        }
                        else
                        {
                            cleanupTimers = true;
                        }
                    }

                    if (cleanupTimers)
                        timers.RemoveAll(t => t.Disposed);
                }

                Thread.Sleep(TIMER_RESOLUTION);

                config.uptime += TIMER_RESOLUTION;
            }
        }

        private void PostTimerExecution(object state)
        {
            var timer = state as HostTimer;

            if (timer != null && !timer.Disposed && timer.IsEnabled)
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
