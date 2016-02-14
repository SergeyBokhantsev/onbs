using Interfaces;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIModels.MultipurposeModels;

namespace UIModels
{
    public class DashMenuModel : RotaryListModel<object>
    {
        private readonly List<ListItem<object>> menuItems;

        public DashMenuModel(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor, "list", 10)
        {
            ListItem<object>.PrepareItem(hc.SyncContext, ref menuItems, null, ClickHandler, "Video catalog");
            ListItem<object>.PrepareItem(hc.SyncContext, ref menuItems, null, ClickHandler, "");
            ListItem<object>.PrepareItem(hc.SyncContext, ref menuItems, null, ClickHandler, "");

            UpdateLabels();
        }

        protected override IList<ListItem<object>> QueryItems(int skip, int take)
        {
            return menuItems;
        }

        private void ClickHandler(object sender, EventArgs e)
        {
            if (sender == menuItems[0])
            {
                hc.GetController<IUIController>().ShowPage("DashCamCatalog", null, null);
            }
            else if (sender == menuItems[1])
            {
                hc.Config.InvertBoolSetting(ConfigNames.DashCamRecorderEnabled);
                UpdateLabels();
            }
            else if (sender == menuItems[2])
            {
                hc.Config.InvertBoolSetting(ConfigNames.DashCamRecorderPreviewEnabled);
                UpdateLabels();
            }
        }

        private void UpdateLabels()
        {
            menuItems[1].Caption = string.Concat("Recordings: ", hc.Config.GetBool(ConfigNames.DashCamRecorderEnabled) ? "enabled" : "DISABLED");
            menuItems[2].Caption = string.Concat("Preview: ", hc.Config.GetBool(ConfigNames.DashCamRecorderPreviewEnabled) ? "ON" : "OFF");
        }

    }
}
