using Interfaces;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIModels
{
    public class ZXEmulatorModel : ExternalApplicationPage
    {
        public ZXEmulatorModel(string viewName, IHostController hc, MappedPage pageDescriptor, object arg)
            : base(viewName, hc, pageDescriptor, arg as IProcessRunner)
        {
        }
    }
}
