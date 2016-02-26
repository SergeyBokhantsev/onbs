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
		private bool onExiting;

        public DashPlayerModel(string viewName, IHostController hc, MappedPage pageDescriptor, object arg)
            : base(viewName, hc, pageDescriptor, arg as IProcessRunner)
        {
        }

		protected override void DoAction (string name, PageModelActionEventArgs actionArgs)
		{
			switch (name)
			{
			case "Exit":
				if (onExiting) {
					hc.GetController<IUIController> ()
						.ShowPage ("DashFileOptions", null, null);
				} else {
					Runner.SendToStandardInput ((char)27);
					onExiting = true;
				}
				break;

			default:
				base.DoAction (name, actionArgs);
				break;
			}
		}
    }
}
