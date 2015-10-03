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
        private double? fuelFlow;

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
            while (!Disposed)
            {
                rpm = elm327.GetRPM();
                fuelFlow = elm327.GetFuelFlow();
                hc.Dispatcher.Invoke(this, null, PassToUI);
            }
        }

        private void PassToUI(object sender, EventArgs e)
        {
            if (!Disposed)
            {
                SetProperty("rpm", rpm.HasValue ? (double)rpm.Value : 0d);
                SetProperty("flow", fuelFlow.HasValue ? fuelFlow.Value : 0d);
                SetProperty("refresh", null);
            }
        }
    }
}
