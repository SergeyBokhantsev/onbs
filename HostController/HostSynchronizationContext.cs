using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Interfaces;

namespace HostController
{
    public class HostSynchronizationContext : SynchronizationContext
    {
        private abstract class WorkItem
        {
            protected readonly SendOrPostCallback SendOrPostCallback;
            protected readonly object State;

            public Exception Exception { get; private set; }

            protected WorkItem(SendOrPostCallback sendOrPostCallback, object state)
            {
                SendOrPostCallback = sendOrPostCallback;
                State = state;
            }

            public void Execute()
            {
                try
                {
                    DoExecute();
                }
                catch (Exception ex)
                {
                    Exception = ex;
                }
            }

            protected abstract void DoExecute();
        }

        //SEND
        private class SynchronousWorkItem : WorkItem
        {
            private readonly AutoResetEvent resetEvent;

            public SynchronousWorkItem(SendOrPostCallback sendOrPostCallback, object state, AutoResetEvent resetEvent)
                : base(sendOrPostCallback, state)
            {
                if (resetEvent == null)
                    throw new ArgumentNullException("resetEvent");

                this.resetEvent = resetEvent;
            }

            protected override void DoExecute()
            {
                try
                {
                    SendOrPostCallback(State);
                }
                finally
                {
                    resetEvent.Set();
                }
            }
        }

        //POST
        private class AsynchronousWorkItem : WorkItem
        {
            public AsynchronousWorkItem(SendOrPostCallback sendOrPostCallback, object state)
                : base(sendOrPostCallback, state)
            {
            }

            protected override void DoExecute()
            {
                SendOrPostCallback(State);
            }
        }

        private readonly ILogger logger;
        private readonly AutoResetEvent pumpResetEvent;
        private readonly ConcurrentQueue<WorkItem> pumpItems;
        private Thread ownerThread;

        private bool disposed;
        private readonly object locker = new object();

        private double loadSum;
        private int loadNum;
        private const int loadApproxNum = 10;

        public double Load { get; private set; }

        public HostSynchronizationContext(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");

            this.logger = logger;

            pumpResetEvent = new AutoResetEvent(false);
            pumpItems = new ConcurrentQueue<WorkItem>();
        }

        public void Pump()
        {
            ownerThread = Thread.CurrentThread;
            SetSynchronizationContext(this);

            var fullCycleWatch = new Stopwatch();
            var iddleWatch = new Stopwatch();

            while (!disposed)
            {
                fullCycleWatch.Restart();

                WorkItem workItem;

                while (pumpItems.TryDequeue(out workItem))
                {
                    workItem.Execute();

                    if (workItem.Exception != null)
                        logger.Log(this, workItem.Exception);
                }

                pumpResetEvent.Reset();
                iddleWatch.Restart();
                pumpResetEvent.WaitOne();

                fullCycleWatch.Stop();
                iddleWatch.Stop();

                var currentLoad = (fullCycleWatch.ElapsedTicks - iddleWatch.ElapsedTicks) / (double)fullCycleWatch.ElapsedTicks;
                loadSum += currentLoad;
                loadNum++;

                if (loadNum == loadApproxNum)
                {
                    Load = loadSum / loadNum;
                    loadSum = 0;
                    loadNum = 0;
                }

                logger.LogIfDebug(this, string.Format("Current load: {0}", currentLoad));
            }

            pumpResetEvent.Dispose();
        }

        /// <summary>
        /// Synchronous execution in Host main thread
        /// </summary>
        public override void Send(SendOrPostCallback action, object state)
        {
            using (var resetEvent = new AutoResetEvent(false))
            {
                var item = new SynchronousWorkItem(action, state, resetEvent);
                pumpItems.Enqueue(item);
                pumpResetEvent.Set();

                resetEvent.WaitOne();

                if (item.Exception != null && Thread.CurrentThread != ownerThread)
                    throw item.Exception;
            }
        }

        /// <summary>
        /// Asynchronous execution in Host main thread
        /// </summary>
        public override void Post(SendOrPostCallback action, object state)
        {
            pumpItems.Enqueue(new AsynchronousWorkItem(action, state));
            pumpResetEvent.Set();
        }

        public void Stop()
        {
            lock (locker)
            {
                if (!disposed)
                {
                    disposed = true;
                    pumpResetEvent.Set();
                }
            }
        }
    }
}
