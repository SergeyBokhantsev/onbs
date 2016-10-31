using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationController
{
    internal interface IAutomationToolAdapter
    {
        Task Key(params AutomationKeys[] key);
        Task MouseMove(int x, int y);
		Task MouseClick(AutomationMouseClickTypes type);
    }
}
