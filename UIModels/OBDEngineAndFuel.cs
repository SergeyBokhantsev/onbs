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

        private int? rpm;
        private int? speed;
        private int? coolantTemp;
        private int? engineLoad;
        private int? throttlePosition;

        public OBDEngineAndFuel(IHostController hc)
            :base(hc, typeof(OBDEngineAndFuel).Name)
        {
            elm327 = hc.GetController<IElm327Controller>();

            var elmThread = new Thread(RequestElm);
            elmThread.IsBackground = true;
            elmThread.Start();

            SetProperty("secondary1prefix", "t:");
            //SetProperty("secondary2prefix", "load:");
            SetProperty("secondary2prefix", "thr:");

            SetProperty("secondary1suffix", "°C");
           // SetProperty("secondary2suffix", "%");
            SetProperty("secondary2suffix", "%");
        }

        private void RequestElm()
        {
            int counter = 0;

            while (!Disposed)
            {
                counter++;

                rpm = elm327.GetRPM();
                speed = elm327.GetSpeed();
                engineLoad = elm327.GetEngineLoad();

                hc.Dispatcher.Invoke(this, null, UpdatePrimaryValues);

                if (counter == 10)
                {
                    coolantTemp = elm327.GetCoolantTemp();
                    throttlePosition = elm327.GetThrottlePosition();
                    hc.Dispatcher.Invoke(this, null, UpdateSecondaryValues);
                    counter = 0;
                }
            }
        }

        private void UpdatePrimaryValues(object sender, EventArgs e)
        {
            if (!Disposed)
            {
                SetProperty("primary2", rpm.HasValue ? (double)rpm.Value : 0d);
                SetProperty("primary1", engineLoad.HasValue ? (double)engineLoad.Value : 0d);
                SetProperty("primary3", speed.HasValue ? (double)speed.Value : 0d);
                SetProperty("refresh", null);
            }
        }

        private void UpdateSecondaryValues(object sender, EventArgs e)
        {
            if (!Disposed)
            {
                SetProperty("secondary1", coolantTemp.HasValue ? (double)coolantTemp.Value : 0d);
                //SetProperty("secondary2", engineLoad.HasValue ? (double)engineLoad.Value : 0d);
                SetProperty("secondary2", throttlePosition.HasValue ? (double)throttlePosition.Value : 0d);
                SetProperty("refresh", null);
            }
        }
    }
}
