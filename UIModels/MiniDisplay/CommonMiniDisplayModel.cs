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
        private readonly IConfig config;
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
            warning = false;

            mdc.ResetQueue();

            var g = mdc.Graphics;

            g.Cls();

            DrawTick(g);

            DrawStatus(g);

            if (!string.IsNullOrWhiteSpace(pageName))
                g.Print(0, 64 - 10, pageName, TextAlingModes.Center);

            DrawClient(g);

            if (warning && DateTime.Now > warningBlinkTime.AddSeconds(5))
            {
                g.Invert();
                g.Update();
                g.Delay(200);
                g.Invert();
                warningBlinkTime = DateTime.Now;
            }

            g.Update();
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

            if (config.IsMessagePending)
            {
                g.Print(0, y, "M");
                y += 12;
                warning = true;
            }

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
        }

        protected abstract void DrawClient(IMiniDisplayGraphics graphics);
    }
}
