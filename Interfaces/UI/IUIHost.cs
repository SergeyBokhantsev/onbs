using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.UI
{
    public interface IUIHost
    {
        void ShowPage(IPageModel model);
    }
}
