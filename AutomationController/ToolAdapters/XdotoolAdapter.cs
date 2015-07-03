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
            return processRunnerFactory.Create(automationTool, args, waitForUI);
        }
    }
}








