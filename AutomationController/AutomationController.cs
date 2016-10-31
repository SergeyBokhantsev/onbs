using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationController
{
    public class AutomationController : IAutomationController
    {
        private readonly IAutomationToolAdapter tool;

        public AutomationController(IHostController hostController)
        {
            var automationTool = hostController.Config.GetString("automation_tool");

            switch (automationTool)
            {
                case "xdotool":
                    tool = new ToolAdapters.XdotoolAdapter(hostController.Logger);
                    break;

                case "stub":
                    tool = new ToolAdapters.StubAdapter();
                    break;

                default:
                    throw new NotImplementedException(automationTool);
            }
        }

        public async Task Key(params AutomationKeys[] key)
        {
            await tool.Key(key);
        }

        public async Task MouseMove(int x, int y)
        {
            await tool.MouseMove(x, y);
        }

        public async Task MouseClick(AutomationMouseClickTypes type)
		{
		 	await tool.MouseClick(type);
		}
    }
}
