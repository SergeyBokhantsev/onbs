using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interfaces;

namespace HostController
{
    public class Dispatcher : IDispatcher
    {
        private class InvokeItem
        {
            private EventHandler handler;
            private EventArgs args;
            private object sender;

            public InvokeItem(object sender, EventArgs args, EventHandler handler)
            {
                if (handler == null)
                    throw new ArgumentException("Handler");

                this.handler = handler;
                this.args = args;
                this.sender = sender;
            }

            public void Invoke()
            {
                handler(sender, args);
            }

            public override string ToString()
            {
                return handler.Method.ToString();
            }
        }

        private readonly List<DispatcherTimer> timers = new List<DispatcherTimer>();
        private readonly ManualResetEventSlim mre = new ManualResetEventSlim(false);
        private readonly Queue<InvokeItem> invokeItems = new Queue<InvokeItem>();
        private readonly ILogger logger;
        private readonly int threadId;

        private bool exit;
                
        public Dispatcher(ILogger logger)
        {
            this.threadId = Thread.CurrentThread.ManagedThreadId;
            this.logger = logger;

            logger.Log(this, "Dispatcher created", LogLevels.Info);
        }

        public void Invoke(object sender, EventArgs args, EventHandler handler)
        {
            AssertCallerIsNotNull(sender);

            lock (invokeItems)
            {
                invokeItems.Enqueue(new InvokeItem(sender, args, handler));
                mre.Set();
            }

           // logger.LogIfDebug(this, string.Format("Invoke call from {0} with handler '{1}' sheduled", sender ?? "NULL", handler.Method));
        }

        [Conditional("DEBUG")]
        private void AssertCallerIsNotNull(object caller)
        {
            if (caller == null)
                throw new ArgumentNullException("Dispatcher caller is null");
        }

        public void Run()
        {
            while (!exit)
            {
                if (mre.Wait(100))
                {
                    InvokeItem itemToInvoke = null;

                    lock (invokeItems)
                    {
                        if (invokeItems.Any())
                        {
                            itemToInvoke = invokeItems.Dequeue();
                        }

                        if (!invokeItems.Any())
                            mre.Reset();
                    }

                    if (itemToInvoke != null)
                    {
                        try
                        {
                            itemToInvoke.Invoke();
                           // logger.LogIfDebug(this, string.Format("Invoke handler '{0}' executed", itemToInvoke));
                        }
                        catch (Exception ex)
                        {
                            logger.Log(this, string.Format("Invoke handler '{0}' throw exeption", itemToInvoke), LogLevels.Debug);
                            logger.Log(this, ex);
                        }
                    }
                }

                ProcessTimers();
            }
        }

        private void ProcessTimers()
        {
            bool needToCleanDisposed = false;
            var time = DateTime.Now;

            lock (timers)
            {
                foreach (var timer in timers)
                {
                    if (timer.Disposed)
                    {
                        needToCleanDisposed = true;
                    }
                    else
                    {
                        EventHandler timerCallback;
                        if (timer.GetCallbackIfTimeTo(time, out timerCallback))
                        {
                            Invoke(timer, null, timerCallback);
                        }
                    }
                }

                if (needToCleanDisposed)
                {
                    timers.RemoveAll(t => t.Disposed);
                }
            }
        }

        public void Exit()
        {
            exit = true;
            mre.Set();
        }

        public bool Check()
        {
            return Thread.CurrentThread.ManagedThreadId == threadId;
        }

        public IDispatcherTimer CreateTimer(int spanSeconds, EventHandler callback)
        {
            var timer = new DispatcherTimer(spanSeconds, callback);
            timers.Add(timer);
            return timer;
        }
    }

    public class DispatcherTimer: IDispatcherTimer
    {
        private readonly EventHandler callback;
        private DateTime lastExecutedTime;

        private volatile int span;
        private volatile bool enabled;
        private volatile bool disposed;

        public bool Disposed
        {
            get
            {
                return disposed;
            }
            private set
            {
                disposed = value;
            }
        }

        public int Span
        {
            get
            {
                return span;
            }
            set
            {
                span = value;
            }
        }

        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
            }
        }

        public DispatcherTimer(int span, EventHandler callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            this.callback = callback;
            Span = span;
        }

        public bool GetCallbackIfTimeTo(DateTime time, out EventHandler callback)
        {
            if (!Disposed && Enabled && (time - lastExecutedTime).TotalMilliseconds >= Span)
            {
                lastExecutedTime = time;
                callback = this.callback;
                return true;
            }
            else
            {
                callback = null;
                return false;
            }
        }

        public void Dispose()
        {
            Disposed = true;
        }
    }
}
