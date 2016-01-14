using Interfaces;
using Interfaces.MiniDisplay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIModels.MiniDisplay
{
    internal class DriveMiniDisplayModel : CommonMiniDisplayModel
    {
        private enum Modes { AirTemp, Travel, EngineTemp, Last }

        private Modes mode;
        
        public int BufferedPoints { get; set; }
        public int SendedPoints { get; set; }
        public int EngineTemp { get; set; }
        public int AirTemp { get; set; }

        public DriveMiniDisplayModel(IHostController hc, string pageName)
            :base(hc, pageName)
        {
            EngineTemp = int.MinValue;
        }

        protected override void DrawClient(Interfaces.MiniDisplay.IMiniDisplayGraphics g)
        {
            var caption = string.Empty;
            var value = string.Empty;

            switch(mode)
            {
                case Modes.AirTemp:
                    caption = "AIR t";
                    value = AirTemp > int.MinValue ? AirTemp.ToString() : "-";
                    break;

                case Modes.Travel:
                    caption = "TRAVEL";
                    value = string.Format("{0}-{1}", BufferedPoints, SendedPoints);
                    break;

                case Modes.EngineTemp:
                    caption = "ENGINE t";
                    value = EngineTemp > int.MinValue ? EngineTemp.ToString() : "-";
                    break;
            }

            g.Print(0, 8, caption, TextAlingModes.Center);
            g.SetFont(Fonts.BigNumbers);
            g.Print(0, 20, value, TextAlingModes.Center);

            mode = (Modes)((int)mode + 1);
            if (mode == Modes.Last)
                mode = (Modes)0;
        }
    }
}
