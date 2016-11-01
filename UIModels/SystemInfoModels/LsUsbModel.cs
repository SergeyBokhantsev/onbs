using Interfaces;
using Interfaces.UI;

namespace UIModels
{
    public class LsUsbModel : ExecuteToolModel
    {
        public LsUsbModel(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor, "lsusb", null, false)
        {
            FontSize = 20000;
        }
    }
}
