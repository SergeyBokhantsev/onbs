using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UIModels
{
    public class OBDEngineAndFuel : CommonPageBase
    {
        private readonly IElm327Controller elm327;

        ChartOfDouble rpm = new ChartOfDouble { Title = "RPM", Scale = 5000 };
        ChartOfDouble load = new ChartOfDouble { Title = "Load", UnitText = "%", Scale = 100 };
        ChartOfDouble speed = new ChartOfDouble { Title = "Speed", UnitText = "km/h", Scale = 100 };
        ChartOfDouble coolant = new ChartOfDouble { Title = "C-temp", UnitText = "°", Scale = 100 };
        ChartOfDouble throttle = new ChartOfDouble { Title = "Thr.", UnitText = "%", Scale = 100 };

        public OBDEngineAndFuel(IHostController hc)
            :base(hc, typeof(OBDEngineAndFuel).Name)
        {
            elm327 = hc.GetController<IElm327Controller>();

            var elmThread = new Thread(RequestElm);
            elmThread.IsBackground = true;
            elmThread.Start();

            SetProperty("primary1", rpm);
            SetProperty("primary2", load);
            SetProperty("primary3", speed);
            SetProperty("secondary1", coolant);
            SetProperty("secondary2", throttle);
        }

        private void RequestElm()
        {
            int secondaryDivider = 9;
            int secondaryCounter = secondaryDivider;

            while (!Disposed)
            {
                rpm.Add(elm327.GetRPM());
                speed.Add(elm327.GetSpeed());
                load.Add(elm327.GetEngineLoad());

                if (secondaryCounter == secondaryDivider)
                {
                    coolant.Add(elm327.GetCoolantTemp());
                    throttle.Add(elm327.GetThrottlePosition());

                    secondaryCounter = 0;
                }
                else
                {
                    coolant.DuplicateLast();
                    throttle.DuplicateLast();

                    secondaryCounter++;
                }

                SetProperty("refresh", null);
            }
        }
    }
}
