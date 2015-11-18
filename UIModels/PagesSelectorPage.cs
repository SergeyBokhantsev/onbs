using Interfaces;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIModels
{
    public class PagesSelectorPage : CommonPageBase
    {
        public PagesSelectorPage(string viewModelName, IHostController hc, MappedPage pageDescriptor)
            :base(viewModelName, hc, pageDescriptor)
        {
        }
    }
}
