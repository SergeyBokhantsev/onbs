using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Interfaces;
using System.Linq;

namespace HostController
{
    public class HostSynchronizationContext : ONBSSyncContext
    {
        private abstract class WorkItem
        {
            protected readonly SendOrPostCallback SendOrPostCallback;
            protected readonly object State;
            private readonly string details;
            private string trace;

            public Exception Exception { get; private set; }

            protected WorkItem(SendOrPostCallback sendOrPostCallback, object state, string details)
            {
                SendOrPostCallback = sendOrPostCallback;
                State = state;
                this.details = details;
                CreateTrace();
            }

            [Conditional("DEBUG")]
            private void CreateTrace()
            {
                var frames = new StackTrace().GetFrames();
                var lines = from frame in frames.Skip(frames.Length > 3 ? 3 : 0)
                        let method = frame.GetMethod()
                        select string.Concat(method.Module.Assembly.GetName().Name, " | ", method.ToString());
                    
                trace = string.Join(Environment.NewLine, lines);
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

            public override string ToString()
            {
                return string.Format("Method: {0}, Arg: {1}, Details: {2},{3} Trace:{3}{4}", SendOrPostCallback.Method, State ?? "NULL", details ?? "NULL", Environment.NewLine, trace ?? "NO TRACE");
            }
        }

        //SEND
        private class SynchronousWorkItem : WorkItem
        {
            private readonly AutoResetEvent resetEvent;

            public SynchronousWorkItem(SendOrPostCallback sendOrPostCallback, object state, AutoResetEvent resetEvent, string details)
                : base(sendOrPostCallback, state, details)
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
            public AsynchronousWorkItem(SendOrPostCallback sendOrPostCallback, object state, string details)
                : base(sendOrPostCallback, state, details)
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

            var itemWatch = new Stopwatch();

            while (!disposed)
            {
                WorkItem workItem;

                while (pumpItems.TryDequeue(out workItem))
                {
                    itemWatch.Restart();

                    if (workItem == null)
                        continue;

                    workItem.Execute();

                    itemWatch.Stop();

                    if (workItem.Exception != null)
                        logger.Log(this, workItem.Exception);

                    if (itemWatch.ElapsedMilliseconds > 1000)
                        logger.Log(this, string.Format("Work item {0} spent {1} milliseconds", workItem, itemWatch.ElapsedMilliseconds), LogLevels.Warning);
                }

                itemWatch.Stop();


				pumpResetEvent.WaitOne();

                
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
                var item = new SynchronousWorkItem(action, state, resetEvent, null);
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
			if (action == null) {
				logger.Log (this, "NULL action provided to POST", LogLevels.Error);
				return;
			}

            Post(action, state, action.Target != null ? action.Target.GetType().FullName : "NO TARGET");
        }

        public override void Post(SendOrPostCallback action, object state, string details)
        {
			if (action == null) {
				logger.Log (this, string.Format("NULL '{0}' action provided to POST", details), LogLevels.Error);
				return;
			}

            pumpItems.Enqueue(new AsynchronousWorkItem(action, state, details));
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
