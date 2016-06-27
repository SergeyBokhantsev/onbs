using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationController.ToolAdapters
{
    public class XdotoolAdapter : IAutomationToolAdapter
    {
        private readonly IProcessRunnerFactory processRunnerFactory;
        private readonly ILogger logger;

        private const string automationTool = "xdotool";
        private readonly bool waitForUI = false;

        public XdotoolAdapter(IProcessRunnerFactory processRunnerFactory, ILogger logger)
        {
            if (processRunnerFactory == null
                || logger == null)
                throw new ArgumentNullException("arguments");

            this.processRunnerFactory = processRunnerFactory;
            this.logger = logger;
        }

        public void Key(params Interfaces.AutomationKeys[] key)
        {
            CreateRunner(key).Run();
        }

        private IProcessRunner CreateRunner(params AutomationKeys[] key)
        {
            var args = string.Concat("key ", string.Join("+", key));
            var config = new ProcessConfig 
            { 
                ExePath = automationTool, 
                Args = args, 
                WaitForUI = waitForUI, 
                Silent = true, 
                RedirectStandardInput = false,
                RedirectStandardOutput = false
            };
            return processRunnerFactory.Create(config);
        }

        public void MouseMove(int x, int y)
        {
            var args = string.Format("mousemove {0} {1}", x, y);
            var config = new ProcessConfig
            {
                ExePath = automationTool,
                Args = args,
                WaitForUI = waitForUI,
                Silent = true,
                RedirectStandardInput = false,
                RedirectStandardOutput = false
            };
            processRunnerFactory.Create(config).Run();
        }

		public void MouseClick(AutomationMouseClickTypes type)
		{
			var args = string.Format("click {0}", (int)type);
            var config = new ProcessConfig
            {
                ExePath = automationTool,
                Args = args,
                WaitForUI = waitForUI,
                Silent = true,
                RedirectStandardInput = false,
                RedirectStandardOutput = false
            };
			processRunnerFactory.Create(config).Run();
		}
    }
}








