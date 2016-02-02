using Interfaces;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIModels
{
    public class DashPlayerModel : ExternalApplicationPage
    {
        public DashPlayerModel(string viewName, IHostController hc, MappedPage pageDescriptor, object arg)
            : base(viewName, hc, pageDescriptor, arg as IProcessRunner)
        {
        }
    }
}
