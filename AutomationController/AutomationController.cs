﻿using Interfaces;
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
                    tool = new ToolAdapters.XdotoolAdapter(hostController.ProcessRunnerFactory, hostController.Logger);
                    break;

                case "stub":
                    tool = new ToolAdapters.StubAdapter();
                    break;

                default:
                    throw new NotImplementedException(automationTool);
            }
        }

        public void Key(params AutomationKeys[] key)
        {
            tool.Key(key);
        }

        
    }
}