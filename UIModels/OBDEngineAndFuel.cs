using Interfaces;
using OBD;
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
        private readonly OBDProcessor obd;

        ChartOfDouble rpmChart = new ChartOfDouble { Title = "RPM", Scale = 5000 };
        ChartOfDouble loadChart = new ChartOfDouble { Title = "Load", UnitText = "%", Scale = 100 };
        ChartOfDouble speedChart = new ChartOfDouble { Title = "Speed", UnitText = "km/h", Scale = 100 };
        ChartOfDouble coolantChart = new ChartOfDouble { Title = "C-temp", UnitText = "C°", Scale = 100 };
        ChartOfDouble throttleChart = new ChartOfDouble { Title = "Thr.", UnitText = "%", Scale = 100 };
        ChartOfDouble mapChart = new ChartOfDouble { Title = "MAP", UnitText = "kPa", Scale = 255 };
        ChartOfDouble iatChart = new ChartOfDouble { Title = "IAT", UnitText = "C°", Scale = 100 };
        ChartOfDouble fuelFlowChart = new ChartOfDouble { Title = "FFl.", UnitText = "l/h", Scale = 50 };

        public OBDEngineAndFuel(IHostController hc)
            :base(hc, typeof(OBDEngineAndFuel).Name)
        {
            var elm327 = hc.GetController<IElm327Controller>();
            obd = new OBDProcessor(elm327);

            SetProperty("primary1", rpmChart);
            SetProperty("primary2", loadChart);
            SetProperty("primary3", fuelFlowChart);
            SetProperty("secondary1", coolantChart);
            SetProperty("secondary2", iatChart);
            SetProperty("secondary3", mapChart);
            SetProperty("secondary4", throttleChart);
            SetProperty("secondary5", speedChart);
            SetProperty("secondary6", fuelFlowChart);

            var elmThread = new Thread(RequestElm);
            elmThread.IsBackground = true;
            elmThread.Start();
        }

        private void RequestElm()
        {
            int secondaryDivider = 9;
            int secondaryCounter = secondaryDivider;

            while (!Disposed)
            {
                rpmChart.Add(obd.GetRPM());
                speedChart.Add(obd.GetSpeed());
                loadChart.Add(obd.GetEngineLoad());
                mapChart.Add(obd.GetMAP());
                fuelFlowChart.Add(obd.GetFuelFlowPerHour(mapChart.Last, rpmChart.Last, iatChart.Last));

                if (secondaryCounter == secondaryDivider)
                {
                    coolantChart.Add(obd.GetCoolantTemp());
                    throttleChart.Add(obd.GetThrottlePosition());
                    iatChart.Add(obd.GetIntakeAirTemp());

                    secondaryCounter = 0;
                }
                else
                {
                    coolantChart.DuplicateLast();
                    throttleChart.DuplicateLast();
                    iatChart.DuplicateLast();

                    secondaryCounter++;
                }

                SetProperty("refresh", null);
            }
        }
    }
}
