using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.UI;
using UIModels.MultipurposeModels;

namespace UIModels
{
    public class TestListModel : RotaryListModel<int>
    {
        public TestListModel(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor, "list", 2)
        {
        }

        protected override IList<ListItem<int>> QueryItems(int skip, int take)
        {
            var range = Enumerable.Range(0, 5);

            List<ListItem<int>> list = null;

            ListItem<int>.PrepareItems(hc.SyncContext, ref list, range.Skip(skip).Take(take), OnClick, i => "item_" + i.ToString());

            return list;
        }

        private void OnClick(object sender, EventArgs e)
        {
            var item = (ListItem<int>)sender;
            item.Caption = Guid.NewGuid().ToString();
        }
    }
}
