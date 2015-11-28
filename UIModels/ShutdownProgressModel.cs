using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.UI;

namespace UIModels
{
    public class ShutdownProgressModel : MultilineModel
    {
        public ShutdownProgressModel(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor)
        {
            SetProperty(ModelNames.PageTitle, "Shutdown in progress");
        }
    }
}
