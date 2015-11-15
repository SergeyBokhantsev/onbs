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
        void ShowPage(string descriptorName, string viewName);
        void ShowPage(IPageModel model);
        void ShowDefaultPage();
        void ShowDialog(IDialogModel model);
        Task<DialogResults> ShowDialogAsync(IDialogModel model);
    }
}
