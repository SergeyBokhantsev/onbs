using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationController.ToolAdapters
{
    public class StubAdapter : IAutomationToolAdapter
    {
        public void Key(params Interfaces.AutomationKeys[] key)
        {
        }

        public void MouseMove(int x, int y)
        {
        }
    }
}
