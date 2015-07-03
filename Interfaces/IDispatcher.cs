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
        bool Check();
        void Invoke(object sender, EventArgs args, EventHandler handler);

        IDispatcherTimer CreateTimer(int spanMs, EventHandler callback);
    }
}
