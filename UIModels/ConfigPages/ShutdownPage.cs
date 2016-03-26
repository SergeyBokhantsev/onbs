using System.Collections.Generic;
using Interfaces;
using Interfaces.UI;
using UIModels.MultipurposeModels;
using System.Linq;

namespace UIModels
{
    public class ShutdownPage : RotaryListModel<string>
    {
        private readonly List<ListItem<string>> items;

        public ShutdownPage(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor, "list", 10)
        {
            SetProperty(ModelNames.PageTitle, "System power management");

            ListItem<string>.PrepareItem(hc.SyncContext, ref items, "Quit", OnClick, "Quit application");
            ListItem<string>.PrepareItem(hc.SyncContext, ref items, "Update", OnClick, "Firmware update");
            ListItem<string>.PrepareItem(hc.SyncContext, ref items, "Restart", OnClick, "Restart system");
            ListItem<string>.PrepareItem(hc.SyncContext, ref items, "Shutdown", OnClick, "Shutdown");
        }

        private async void OnClick(object sender, System.EventArgs e)
        {
            var action = ((ListItem<string>)sender).Value;

            switch (action)
            {
                case "Quit":
                    hc.Shutdown(HostControllerShutdownModes.Exit);
                    break;

			case "Update":

				var dr = await hc.GetController<IUIController> ().ShowDialogAsync (
					         new Dialogs.YesNoDialog ("Updating", "Pull latest sources and compile?", "Update", "Cancel", hc, 10000, DialogResults.No));

				if (dr == DialogResults.Yes) {
					hc.Shutdown (HostControllerShutdownModes.Update);
				}
                    break;

                case "Restart":
                    hc.Shutdown(HostControllerShutdownModes.Restart);
                    break;

                case "Shutdown":
                    hc.Shutdown(HostControllerShutdownModes.Shutdown);
                    break;
            }
        }

        protected override IList<ListItem<string>> QueryItems(int skip, int take)
        {
            if (skip == 0)
                return items;
            else
                return null;
        }
    }
}
