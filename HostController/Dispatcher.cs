using System;
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

            logger.LogIfDebug(this, string.Format("Invoke call from {0} with handler '{1}' sheduled", sender ?? "NULL", handler.Method));
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
                if (mre.Wait(10000))
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
                            logger.LogIfDebug(this, string.Format("Invoke handler '{0}' executed", itemToInvoke));
                        }
                        catch (Exception ex)
                        {
                            logger.Log(this, string.Format("Invoke handler '{0}' throw exeption", itemToInvoke), LogLevels.Debug);
                            logger.Log(this, ex);
                        }
                    }
                }
                else
                {
                    logger.LogIfDebug(this, "Dispatcher heartbeat");
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
    }
}
