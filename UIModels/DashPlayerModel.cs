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
        public DashPlayerModel(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor, CreateProcessRunner(hc))
        {
        }

        private static IProcessRunner CreateProcessRunner(IHostController hc)
        {
            var config = new ProcessConfig
            {
                 ExePath = hc.Config.GetString(ConfigNames.DashCamPlayerExe),
                 Args = string.Format(hc.Config.GetString(ConfigNames.DashCamPlayerArg), DashCamCatalogModel.SelectedFile.FullName),
                 WaitForUI = false,
				 RedirectStandardInput = false,
				 RedirectStandardOutput = false
            };

            return hc.ProcessRunnerFactory.Create(config);
        }
    }
}
