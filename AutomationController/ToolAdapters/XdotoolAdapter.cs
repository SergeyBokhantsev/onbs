using Interfaces;
using ProcessRunnerNamespace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationController.ToolAdapters
{
    public class XdotoolAdapter : IAutomationToolAdapter
    {
        private readonly ILogger logger;

        private const string automationTool = "xdotool";
        private readonly bool waitForUI = false;

        public XdotoolAdapter(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");

            this.logger = logger;
        }

        private async Task ExecuteRunner(string args)
        {
            ProcessRunner pr = null;

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = automationTool,
                    Arguments = args
                };

                pr = new ProcessRunner(psi, false, false);

                await pr.RunAsync();
            }
            catch (AggregateException ex)
            {
                logger.Log(this, ex.Flatten().InnerException ?? ex);
            }
            catch (Exception ex)
            {
                logger.Log(this, ex);
            }
        }

        public async Task Key(params Interfaces.AutomationKeys[] key)
        {
            await ExecuteRunner(string.Concat("key ", string.Join("+", key)));
        }

        public async Task MouseMove(int x, int y)
        {
            await ExecuteRunner(string.Format("mousemove {0} {1}", x, y));
        }

		public async Task MouseClick(AutomationMouseClickTypes type)
		{
            await ExecuteRunner(string.Format("click {0}", (int)type));
		}
    }
}








