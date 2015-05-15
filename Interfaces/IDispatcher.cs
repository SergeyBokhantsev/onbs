using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IDispatcher
    {
        void Run();
        void Exit();
        void Invoke(object sender, EventArgs args, EventHandler handler);
    }
}
