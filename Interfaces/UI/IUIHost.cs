using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.UI
{
    public interface IUIHost
    {
		void Run(bool fullscreen);
        void ShowPage(IPageModel model);
        void ShowDialog(IPageModel model);
        void Shutdown();
    }
}
