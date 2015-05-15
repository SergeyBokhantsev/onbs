using System;
using System.Collections.Generic;
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

        private readonly ManualResetEvent mre = new ManualResetEvent(false);
        private readonly Queue<InvokeItem> invokeItems = new Queue<InvokeItem>();
        private readonly ILogger logger;

        private bool exit;
                
        public Dispatcher(ILogger logger)
        {
            this.logger = logger;

            logger.Log("Dispatcher created", LogLevels.Debug);
        }

        public void Invoke(object sender, EventArgs args, EventHandler handler)
        {
            lock (invokeItems)
            {
                invokeItems.Enqueue(new InvokeItem(sender, args, handler));
                mre.Set();
            }

            logger.Log(string.Format("Invoke call with handler '{0}' sheduled", handler.Method), LogLevels.Debug);
        }

        public void Run()
        {
            while (!exit)
            {
                mre.WaitOne();
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
                        logger.Log(string.Format("Invoke handler '{0}' executed", itemToInvoke), LogLevels.Debug);
                    }
                    catch (Exception ex)
                    {
                        logger.Log(string.Format("Invoke handler '{0}' throw exeption", itemToInvoke), LogLevels.Debug);
                        logger.Log(ex);
                    }
                }
            }
        }

        public void Exit()
        {
            exit = true;
            mre.Set();
        }
    }
}
