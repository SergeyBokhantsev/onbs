﻿using Interfaces;
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
        private double? fuelFlow;
        private int? coolantTemp;
        private int? engineLoad;

        public OBDEngineAndFuel(IHostController hc)
            :base(hc, typeof(OBDEngineAndFuel).Name)
        {
            elm327 = hc.GetController<IElm327Controller>();

            var elmThread = new Thread(RequestElm);
            elmThread.IsBackground = true;
            elmThread.Start();
        }

        private void RequestElm()
        {
            int counter = 0;

            while (!Disposed)
            {
                counter++;

                rpm = elm327.GetRPM();
                fuelFlow = elm327.GetFuelFlow();
                hc.Dispatcher.Invoke(this, null, UpdatePrimaryValues);

                if (counter == 10)
                {
                    coolantTemp = elm327.GetCoolantTemp();
                    engineLoad = elm327.GetEngineLoad();
                    hc.Dispatcher.Invoke(this, null, UpdateSecondaryValues);
                    counter = 0;
                }
                else
                {
                    Thread.Sleep(200);
                }
            }
        }

        private void UpdatePrimaryValues(object sender, EventArgs e)
        {
            if (!Disposed)
            {
                SetProperty("rpm", rpm.HasValue ? (double)rpm.Value : 0d);
                SetProperty("flow", fuelFlow.HasValue ? fuelFlow.Value : 0d);
                SetProperty("refresh", null);
            }
        }

        private void UpdateSecondaryValues(object sender, EventArgs e)
        {
            if (!Disposed)
            {
                SetProperty("par1", coolantTemp.HasValue ? string.Concat(coolantTemp.Value, "°C") : "--");
                SetProperty("par2", coolantTemp.HasValue ? string.Concat(engineLoad.Value, "%") : "--");
                SetProperty("refresh", null);
            }
        }
    }
}
