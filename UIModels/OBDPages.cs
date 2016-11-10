using Interfaces;
using OBD;
using System;
using System.Linq;
using System.Threading;
using Interfaces.UI;
using System.Diagnostics;
using System.IO;
using UIModels.Dialogs;
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

    public abstract class OBDChartPage : DrivePageBase
    {
        private readonly IElm327Controller elm;
        private readonly OBDChart[] primary;
        private readonly OBDChart[] secondary;
        
        protected static OBDChart CreateRPMChart(OBDProcessor obd)
        {
            return new OBDChart(chart => { var result = obd.GetRPM(); chart.Add(result ?? 0d); }) { Title = "RPM", Scale = 5000 };
        }
        protected static OBDChart CreateLoadChart(OBDProcessor obd)
        {
            return new OBDChart(chart => { var result = obd.GetEngineLoad(); chart.Add(result ?? 0d); }) { Title = "Load", UnitText = "%", Scale = 100 };
        }
        protected static OBDChart CreateSpeedChart(OBDProcessor obd)
        {
            return new OBDChart(chart => { var result = obd.GetSpeed(); chart.Add(result ?? 0d); }) { Title = "Speed", UnitText = "km/h", Scale = 100 };
        }
        protected static OBDChart CreateCoolantTempChart(OBDProcessor obd)
        {
            return new OBDChart(chart => { var result = obd.GetCoolantTemp(); chart.Add(result ?? 0d); }) { Title = "C-temp", UnitText = "C°", Scale = 100 };
        }
        protected static OBDChart CreateThrottleChart(OBDProcessor obd)
        {
            return new OBDChart(chart => { var result = obd.GetThrottlePosition(); chart.Add(result ?? 0d); }) { Title = "Thr.", UnitText = "%", Scale = 100 };
        }
        protected static OBDChart CreateMAPChart(OBDProcessor obd)
        {
            return new OBDChart(chart => { var result = obd.GetMAP(); chart.Add(result ?? 0d); }) { Title = "MAP", UnitText = "kPa", Scale = 255 };
        }
        protected static OBDChart CreateIntakeAirTempChart(OBDProcessor obd)
        {
            return new OBDChart(chart => { var result = obd.GetIntakeAirTemp(); chart.Add(result ?? 0d); }) { Title = "IAT", UnitText = "C°", Scale = 100 };
        }

        private readonly int secondaryDivider;

        protected OBDChartPage(string viewName, IHostController hc, MappedPage pageDescriptor, int secondaryDivider)
            : base(viewName, hc, pageDescriptor)
        {
            elm = hc.GetController<IElm327Controller>();
            var obd = new OBDProcessor(elm);

            // ReSharper disable once VirtualMemberCallInContructor
            this.primary = GetPrimaryCharts(obd);
            // ReSharper disable once VirtualMemberCallInContructor
            this.secondary = GetSecondaryCharts(obd);
            this.secondaryDivider = secondaryDivider;
            
            InitProperties();

            var elmThread = new Thread(RequestElm) {IsBackground = true};
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
            var sw = new Stopwatch();

            while (!Disposed)
            {
                sw.Start();

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

                sw.Stop();

                if (!string.IsNullOrEmpty(elm.Error))
                {
                    hc.Logger.Log(this, string.Format("Resetting Elm327 module. Error was: {0}", elm.Error), LogLevels.Warning);
                    elm.Reset();
                    Thread.Sleep(3000);
                }
                else if (sw.ElapsedMilliseconds < 100)
                {
                    Thread.Sleep(100 - (int)sw.ElapsedMilliseconds);
                }

                sw.Reset();

                SetProperty("refresh", null);
            }
        }
    }

    public class OBDEngine1 : OBDChartPage
    {
        public OBDEngine1(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor, 30)
        {
        }

        protected override OBDChart[] GetPrimaryCharts(OBDProcessor obd)
        {
            return new[] 
            {
                CreateRPMChart(obd),
                CreateLoadChart(obd),
                CreateSpeedChart(obd)
            };
        }

        protected override OBDChart[] GetSecondaryCharts(OBDProcessor obd)
        {
            return new[] 
            {
                CreateCoolantTempChart(obd),
                CreateIntakeAirTempChart(obd)
            };
        }
    }

    public class OBDEngine2 : OBDChartPage
    {
        public OBDEngine2(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor, 30)
        {
        }

        protected override OBDChart[] GetPrimaryCharts(OBDProcessor obd)
        {
            return new[] 
            {
                CreateThrottleChart(obd),
                CreateLoadChart(obd),
                CreateMAPChart(obd)
            };
        }

        protected override OBDChart[] GetSecondaryCharts(OBDProcessor obd)
        {
            return new[] 
            {
                CreateCoolantTempChart(obd),
                CreateIntakeAirTempChart(obd)
            };
        }
    }

    public class OBD_DTCPage : DrivePageBase
    {
        private readonly OBDProcessor obd;

        public OBD_DTCPage(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor)
        {
            var elm327 = hc.GetController<IElm327Controller>();
            obd = new OBDProcessor(elm327);

            hc.SyncContext.Post(async o => await Refresh(), null, "OBD DTC refresh");
        }

        private async Task Refresh()
        {
            SetProperty("codes", "Refreshing...");

            await Task.Run(() =>
            {
                try
                {
                    var dtcFiles = hc.Config.GetString(ConfigNames.OBD_DTC_DescriptionFiles).Split(';').Select(f => Path.Combine(hc.Config.DataFolder, f.Trim())).ToArray();
                    var dtcDescriptor = new DTCDescriptor(dtcFiles);

                    var tcodes = obd.GetTroubleCodes().Select(tc => string.Format("{0}: {1}", tc, dtcDescriptor.GetDescription(tc)));

                    var content = string.Join("\r\n", tcodes);

                    SetProperty("codes", !string.IsNullOrWhiteSpace(content) ? content : "None");
                }
                catch (Exception ex)
                {
                    hc.Logger.Log(this, ex);
                }
            });
        }

        protected override async Task DoAction(string name, PageModelActionEventArgs actionArgs)
        {
            switch (name)
            {
                case "Clear":
                    var confirmation = await hc.GetController<IUIController>().ShowDialogAsync(new YesNoDialog("Reseting DTC", "Please confirm reseting trouble codes", "Reset", "Cancel", hc, 10000, DialogResults.No));
                    if (confirmation == DialogResults.Yes)
                    {
                        hc.Logger.Log(this, "Performing DTC resetting...", LogLevels.Warning);
                        var resetResult = obd.ResetTroubleCodes();
                        SetProperty("codes", resetResult ? "Reset successfull" : "Cannot reset");
                        hc.Logger.Log(this, string.Format("DTC resetting result: {0}", resetResult ? "SUCCESS" : "FAIL"), LogLevels.Warning);
                    }
                    else
                    {
                        hc.Logger.Log(this, "DTC resetting was cancelled by user", LogLevels.Warning);
                    }
                    break;

                case "Refresh":                    
                    await Refresh();
                    break;

                default:
                    await base.DoAction(name, actionArgs);
                    break;
            }
        }
    }
}
