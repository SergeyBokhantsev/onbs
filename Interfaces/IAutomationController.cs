using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    /// <summary>
    /// http://wiki.linuxquestions.org/wiki/List_of_keysyms
    /// </summary>
    public enum AutomationKeys
    {
		a,
		d,
		Tab = 0x0009,
        plus = 0x002b,
        comma = 0x002c,
        minus = 0x002d,
        Down = 0x0600,
        Left = 0x0601,
        Right = 0x0602,
        Up = 0x0603,
        Control = 0x0702,
		Alt = 0x0703,
    };

    public interface IAutomationController : IController
    {
        void Key(params AutomationKeys[] key);
        void MouseMove(int x, int y);
    }
}
