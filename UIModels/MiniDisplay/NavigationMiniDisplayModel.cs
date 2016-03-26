using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces.MiniDisplay;
using Interfaces;

namespace UIModels.MiniDisplay
{
    internal class NavigationMiniDisplayModel : CommonMiniDisplayModel
    {
        public NavigationMiniDisplayModel(IHostController hc, string pageName)
            :base(hc, pageName)
        {
        }

        protected override void DrawClient(IMiniDisplayGraphics graphics)
        {
            graphics.SetFont(Fonts.Small);
            var gpsState = string.Concat("GPS: ", config.GetBool(ConfigNames.GPSDPaused) ? "PAUSED" : "On");
            graphics.Print(0, 20, gpsState, TextAlingModes.Center);
        }
    }
}
