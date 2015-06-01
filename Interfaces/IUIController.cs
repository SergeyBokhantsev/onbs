using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces.UI;

namespace Interfaces
{
    public interface IUIController : IController
    {
        void ShowMainPage();
        void ShowPage(IPageModel model);
    }
}
