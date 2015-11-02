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
    public class OBDChart : ChartOfDouble
    {
        private Action<OBDChart> requestor;

        public OBDChart (Action<OBDChart> requestor)
        {
            if (requestor == null)
                throw new ArgumentNullException("requestor");

            this.requestor = requestor;
        }

        public void Request()
        {
            requestor(this);
        }
    }

    public abstract class OBDChartPage : CommonPageBase
    {
        private readonly OBDProcessor obd;
        private readonly OBDChart[] primary;
        private readonly OBDChart[] secondary;
        
        protected static OBDChart CreateRPMChart(OBDProcessor obd)
        {
            return new OBDChart(chart => { var result = obd.GetRPM(); chart.Add(result.HasValue ? (double)result.Value : 0d); }) { Title = "RPM", Scale = 5000 };
        }
        protected static OBDChart CreateLoadChart(OBDProcessor obd)
        {
            return new OBDChart(chart => { var result = obd.GetEngineLoad(); chart.Add(result.HasValue ? (double)result.Value : 0d); }) { Title = "Load", UnitText = "%", Scale = 100 };
        }
        protected static OBDChart CreateSpeedChart(OBDProcessor obd)
        {
            return new OBDChart(chart => { var result = obd.GetSpeed(); chart.Add(result.HasValue ? (double)result.Value : 0d); }) { Title = "Speed", UnitText = "km/h", Scale = 100 };
        }
        protected static OBDChart CreateCoolantTempChart(OBDProcessor obd)
        {
            return new OBDChart(chart => { var result = obd.GetCoolantTemp(); chart.Add(result.HasValue ? (double)result.Value : 0d); }) { Title = "C-temp", UnitText = "C°", Scale = 100 };
        }
        protected static OBDChart CreateThrottleChart(OBDProcessor obd)
        {
            return new OBDChart(chart => { var result = obd.GetThrottlePosition(); chart.Add(result.HasValue ? (double)result.Value : 0d); }) { Title = "Thr.", UnitText = "%", Scale = 100 };
        }
        protected static OBDChart CreateMAPChart(OBDProcessor obd)
        {
            return new OBDChart(chart => { var result = obd.GetMAP(); chart.Add(result.HasValue ? (double)result.Value : 0d); }) { Title = "MAP", UnitText = "kPa", Scale = 255 };
        }
        protected static OBDChart CreateIntakeAirTempChart(OBDProcessor obd)
        {
            return new OBDChart(chart => { var result = obd.GetIntakeAirTemp(); chart.Add(result.HasValue ? (double)result.Value : 0d); }) { Title = "IAT", UnitText = "C°", Scale = 100 };
        }

        private int secondaryDivider;

        public OBDChartPage(IHostController hc, string name, int secondaryDivider)
            :base(hc, name)
        {
            var elm327 = hc.GetController<IElm327Controller>();
            obd = new OBDProcessor(elm327);

            this.primary = GetPrimaryCharts(obd);
            this.secondary = GetSecondaryCharts(obd);
            this.secondaryDivider = secondaryDivider;
            
            InitProperties();

            var elmThread = new Thread(RequestElm);
            elmThread.IsBackground = true;
            elmThread.Start();
        }

        protected abstract OBDChart[] GetPrimaryCharts(OBDProcessor obd);
        protected abstract OBDChart[] GetSecondaryCharts(OBDProcessor obd);

        private void InitProperties()
        {
            if (primary != null && primary.Any())
            {
                for (int i=0; i<primary.Length; ++i)
                {
                    SetProperty(string.Concat("primary", i+1), primary[i]);
                }
            }

            if (secondary != null && secondary.Any())
            {
                for (int i=0; i<secondary.Length; ++i)
                {
                    SetProperty(string.Concat("secondary", i+1), secondary[i]);
                }
            }
        }

        private void RequestElm()
        {
            int secondaryCounter = secondaryDivider;

            while (!Disposed)
            {
                if (primary != null && primary.Any())
                {
                    for (int i = 0; i < primary.Length; ++i)
                    {
                        primary[i].Request();
                    }
                }

                if (secondaryCounter == secondaryDivider)
                {
                    if (secondary != null && secondary.Any())
                    {
                        for (int i = 0; i < secondary.Length; ++i)
                        {
                            secondary[i].Request();
                        }
                    }

                    secondaryCounter = 0;
                }
                else
                {
                    if (secondary != null && secondary.Any())
                    {
                        for (int i = 0; i < secondary.Length; ++i)
                        {
                            secondary[i].DuplicateLast();
                        }
                    }

                    secondaryCounter++;
                }

                SetProperty("refresh", null);
            }
        }
    }

    public class OBDEngineAndFuel : OBDChartPage
    {
        public OBDEngineAndFuel(IHostController hc)
            :base(hc, typeof(OBDEngineAndFuel).Name, 30)
        {
        }

        protected override OBDChart[] GetPrimaryCharts(OBDProcessor obd)
        {
            return new OBDChart[] 
            {
                CreateRPMChart(obd),
                CreateLoadChart(obd),
                CreateSpeedChart(obd)
            };
        }

        protected override OBDChart[] GetSecondaryCharts(OBDProcessor obd)
        {
            return new OBDChart[] 
            {
                CreateCoolantTempChart(obd),
                CreateIntakeAirTempChart(obd)
            };
        }
    }
}
