using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace AutomationController.ToolAdapters
{
    public class StubAdapter : IAutomationToolAdapter
    {
        public Task Key(params Interfaces.AutomationKeys[] key)
        {
            return Task.FromResult(0);
        }

        public Task MouseMove(int x, int y)
        {
            return Task.FromResult(0);
        }

		public Task MouseClick(AutomationMouseClickTypes type)
		{
            return Task.FromResult(0);
		}
    }
}
