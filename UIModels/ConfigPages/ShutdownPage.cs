using Interfaces;
using Interfaces.UI;

namespace UIModels
{
    public class ShutdownPage : ModelBase
    {
        public ShutdownPage(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor)
        {
            SetProperty(ModelNames.PageTitle, "System power management");
        }

        protected override void DoAction(string name, PageModelActionEventArgs actionArgs)
        {
            switch (name)
            {
                case "Quit":
                    hc.Shutdown(HostControllerShutdownModes.Exit);
                    break;

                case "Restart":
                    hc.Shutdown(HostControllerShutdownModes.Restart);
                    break;

                case "Shutdown":
                    hc.Shutdown(HostControllerShutdownModes.Shutdown);
                    break;

                default:
                    base.DoAction(name, actionArgs);
                    break;
            }
        }
    }
}
