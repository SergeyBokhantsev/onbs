using Interfaces;
using Interfaces.MiniDisplay;
using Interfaces.UI;

namespace UIModels
{
    public class ShutdownProgressModel : MultilineModel
    {
        public ShutdownProgressModel(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor)
        {
            SetProperty(ModelNames.PageTitle, "Shutdown in progress");

            var mdc = hc.GetController<IMiniDisplayController>();
            mdc.ResetQueue();
            mdc.Graphics.Cls();
            mdc.Graphics.SetFont(Fonts.Small);
            mdc.Graphics.Print(0, 25, "SHUTDOWN...", TextAlingModes.Center);
            mdc.Graphics.Update();
        }
    }
}
