using Interfaces;
using Interfaces.UI;

namespace UIModels
{
    public class PagesSelectorPage : DrivePageBase
    {
        public PagesSelectorPage(string viewModelName, IHostController hc, MappedPage pageDescriptor)
            :base(viewModelName, hc, pageDescriptor)
        {
        }
    }
}
