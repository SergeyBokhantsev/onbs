using Interfaces;
using Interfaces.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIModels
{
    public class UnexpectedErrorModel : MultilineModel
    {
        public UnexpectedErrorModel(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor)
        {
            SetProperty(ModelNames.PageTitle, "Unexpected error occured");
        }
    }
}
