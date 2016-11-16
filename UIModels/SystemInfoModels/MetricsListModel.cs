using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.UI;
using UIModels.MultipurposeModels;

namespace UIModels.SystemInfoModels
{
    public class MetricsListModel : RotaryListModel<IMetricsProvider>
    {
        private readonly List<RotaryListModel<IMetricsProvider>.ListItem<IMetricsProvider>> items;

        public MetricsListModel(string viewName, IHostController hc, MappedPage pageDescriptor)
            :base(viewName, hc, pageDescriptor, "list", 10)
        {
            items = new List<ListItem<IMetricsProvider>>();
            RotaryListModel<IMetricsProvider>.ListItem<IMetricsProvider>.PrepareItems(hc.SyncContext, ref items, hc.MetricsService.Providers, OnProviderSelected, p => p.Name);
        }

        private void OnProviderSelected(object sender, EventArgs e)
        {
            var item = sender as RotaryListModel<IMetricsProvider>.ListItem<IMetricsProvider>;
            var provider = item.Value;


        }

        protected override IList<RotaryListModel<IMetricsProvider>.ListItem<IMetricsProvider>> QueryItems(int skip, int take)
        {
            return items.Skip(skip).Take(take).ToList();
        }
    }
}
