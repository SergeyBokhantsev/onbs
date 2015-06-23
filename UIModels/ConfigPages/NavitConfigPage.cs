using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace UIModels.ConfigPages
{
    public class NavitConfigPage : ModelBase
    {
        private readonly IHostController hc;

        public NavitConfigPage(IHostController hc)
            :base("SystemConfigurationPage", hc.Dispatcher, hc.Logger)
        {
            this.hc = hc;


        }

        protected override void DoAction(Interfaces.UI.PageModelActionEventArgs actionArgs)
        {
            
        }
    }
}
