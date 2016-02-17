using System.Collections.Generic;
using Interfaces;
using Interfaces.UI;
using UIModels.MultipurposeModels;
using System.Linq;

namespace UIModels
{
    public class PagesSelectorPage : RotaryListModel<MappedActionBase>
    {
        private readonly List<ListItem<MappedActionBase>> rotaryItems = new List<ListItem<MappedActionBase>>(10);

        public PagesSelectorPage(string viewModelName, IHostController hc, MappedPage pageDescriptor)
            :base(viewModelName, hc, pageDescriptor, "list", 10)
        {
            foreach (var mappedAction in pageDescriptor.ButtonsMap)
            {
                if (mappedAction.ButtonActionName.StartsWith("RotaryItem_"))
                {
                    ListItem<MappedActionBase>.PrepareItem(hc.SyncContext, ref rotaryItems, mappedAction, (s, e) => Action(new PageModelActionEventArgs(mappedAction.ButtonActionName, Interfaces.Input.ButtonStates.Press)), mappedAction.Caption);
                }
            }
        }

        protected override System.Collections.Generic.IList<ListItem<MappedActionBase>> QueryItems(int skip, int take)
        {
            return rotaryItems.Skip(skip).Take(take).ToList();
        }
    }
}
