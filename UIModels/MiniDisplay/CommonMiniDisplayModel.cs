using Interfaces;
using Interfaces.MiniDisplay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIModels.MiniDisplay
{
    internal abstract class CommonMiniDisplayModel
    {
        private readonly IMiniDisplayController mdc;
        protected readonly IConfig config;
        private readonly string pageName;

        private bool tick;
        protected bool warning;
        private DateTime warningBlinkTime;

        protected CommonMiniDisplayModel(IHostController hc, string pageName)
        {
            if (hc == null)
                throw new ArgumentNullException("hc");

            this.mdc = hc.GetController<IMiniDisplayController>();
            this.config = hc.Config;
            this.pageName = pageName;
        }

        public void Draw()
        {
            if (config.IsMessageShown)
            {
                return;
            }

            warning = false;

            mdc.ResetQueue();

            var g = mdc.Graphics;

            g.Cls();

           // DrawTick(g);

            DrawStatus(g);

            if (!string.IsNullOrWhiteSpace(pageName))
                g.Print(0, 64 - 10, pageName, TextAlingModes.Center);
            
            if (config.IsMessagePending)
            {
                DrawMessageWarning(g);
                warning = true;
            }
            else
            {
                DrawClient(g);
            }

            if (warning && DateTime.Now > warningBlinkTime.AddSeconds(5))
            {
                g.Invert(true);
                g.Delay(200);
                g.Invert(false);
                warningBlinkTime = DateTime.Now;
            }

            g.Update();
        }

        private void DrawMessageWarning(IMiniDisplayGraphics g)
        {
            g.DrawRect(25, 0, 102, 50);
            g.DrawRect(26, 1, 101, 49);
            g.DrawRect(27, 2, 100, 48);
            g.DrawRect(28, 3, 99, 47);

            g.DrawLine(25, 0, 64, 25);
            g.DrawLine(25, 1, 64, 26);
            g.DrawLine(25, 2, 64, 27);

            g.DrawLine(64, 25, 102, 0);
            g.DrawLine(64, 26, 102, 1);
            g.DrawLine(64, 27, 102, 2);
        }

        private void DrawTick(IMiniDisplayGraphics g)
        {
            if (tick)
            {
                g.DrawRect(0, 0, 8, 3);
                g.DrawRect(1, 1, 7, 2);
            }

            tick = !tick;
        }

        private void DrawStatus(IMiniDisplayGraphics g)
        {
            g.SetFont(Fonts.Small);

            byte y = 8;

            if (!config.IsInternetConnected)
            {
                g.Print(0, y, "I");
                y += 12;
                warning = true;
            }

            if (!config.IsGPSLock)
            {
                g.Print(0, y, "G");
                y += 12;
                warning = true;
            }

            if (!config.GetBool(ConfigNames.DashCamRecorderEnabled))
            {
                g.Print(0, y, "D");
                y += 12;
                warning = true;
            }
        }

        protected abstract void DrawClient(IMiniDisplayGraphics graphics);
    }
}
