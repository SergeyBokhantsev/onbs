using Interfaces;
using Interfaces.GPS;
using Interfaces.MiniDisplay;
using Interfaces.UI;
using OBD;
using System;
using System.Linq;
using UIModels.MiniDisplay;
using YandexServicesProvider;
using System.Threading;
using System.IO;
using UIModels.MultipurposeModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UIModels
{
    public class DrivePage : DrivePageBase
    {
        private class DriveStatusReporter
        {
            private readonly ILogger logger;
            private readonly ISpeakService speaker;

            private double lastSpeed;
            private bool engineSpeedWarning;

            public DriveStatusReporter(ILogger logger, ISpeakService speaker)
            {
                this.logger = logger;
                this.speaker = speaker;
            }

            public void SetSpeed(double speed)
            {
                if (speed > 15)
                {
                    if (speed % 10d < 0.6)
                    {
                        var rounded = Math.Floor(speed);

                        if (lastSpeed != rounded)
                        {
                            lastSpeed = rounded;

                            speaker.Speak(lastSpeed.ToString("0"));
                        }
                    }
                }
            }

            public void SetRPM(int rpm)
            {
                if (rpm > 3200 && !engineSpeedWarning)
                {
                    engineSpeedWarning = true;
                    speaker.Speak("Engine speed");
                }

                if (rpm < 2800)
                    engineSpeedWarning = false;
            }
        }

        private static int focusedItemIndex;

        private readonly ITravelController tc;

        private readonly WeatherProvider weather;
        private readonly GeocodingProvider geocoder;

        private readonly IOperationGuard weatherGuard = new InterlockedGuard();
        private readonly IOperationGuard geocoderGuard = new TimedGuard(new TimeSpan(0, 0, 3));
        private readonly IOperationGuard obdGuard = new InterlockedGuard();
        private readonly IOperationGuard engineCoolantTempGuard = new TimedGuard(new TimeSpan(0, 0, 30));
        private readonly IOperationGuard minidisplayGuard = new TimedGuard(new TimeSpan(0, 0, 2));
        private readonly IOperationGuard cpuInfoGuard = new TimedGuard(new TimeSpan(0, 0, 10));

        private readonly DriveMiniDisplayModel miniDisplayModel;

		private readonly IElm327Controller elm;
        private readonly OBDProcessor obdProcessor;

        private readonly DriveStatusReporter reporter;

        public DrivePage(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor, focusedItemIndex)
        {
            elm = hc.GetController<IElm327Controller>();
            obdProcessor = new OBDProcessor(elm);

            miniDisplayModel = new DriveMiniDisplayModel(hc, pageDescriptor.Name);

            this.tc = hc.GetController<ITravelController>();

            this.weather = new WeatherProvider(hc.Logger, hc.Config.DataFolder);
            this.geocoder = new GeocodingProvider();

			SetProperty("oil_temp_icon", Path.Combine(hc.Config.DataFolder, "icons", "OilTemp.png"));

			SetProperty("gear", "-");

            gpsController.GPRMCReseived += GPRMCReseived;

            reporter = new DriveStatusReporter(hc.Logger, hc.SpeakService);
        }

        protected override void OnSecondaryTimer(IHostTimer timer)
        {
            if (!Disposed)
            {
                //weatherGuard.ExecuteIfFreeAsync(UpdateWeatherForecast);
            }

			//hc.GetController<IDashCamController> ().ScheduleTakePicture (pict);

            base.OnSecondaryTimer(timer);
        }

		void pict(MemoryStream p)
		{
			if (p != null) {

				//var arr = p.ToArray ();

				//File.WriteAllBytes (Path.Combine (hc.Config.DataFolder, "t4.jpg"), arr);

			}
		}

        protected override async Task DoAction(string name, PageModelActionEventArgs actionArgs)
        {
            switch (name)
            {
                case "ProtectCurrentDashClip":
                    hc.GetController<IDashCamController>().ProtectDeletion(null);
                    break;

                default:
                    await base.DoAction(name, actionArgs);
                    break;
            }
        }

        private async Task UpdateOBD()
        {
            if (!Disposed)
            {
                await Task.Run(() =>
                    {
                        engineCoolantTempGuard.ExecuteIfFree(() =>
                        {
                             var engineTemp = obdProcessor.GetCoolantTemp() ?? int.MinValue;
                             miniDisplayModel.EngineTemp = engineTemp;
                             SetProperty("eng_temp", engineTemp);
                        });

                        var speed = obdProcessor.GetSpeed();
                        var rpm = obdProcessor.GetRPM();

                        if (speed.HasValue && rpm.HasValue && speed.Value > 0)
                        {
                            var ratio = ((double)rpm.Value / (double)speed.Value);
                            var gear = "-";

                            if (ratio >= 100)
                                gear = "1";
                            else if (ratio >= 67 && ratio < 100)
                                gear = "2";
                            else if (ratio >= 47 && ratio < 67)
                                gear = "3";
                            else if (ratio >= 35 && ratio < 47)
                                gear = "4";
                            else
                                gear = "5";

                            SetProperty("gear", gear);
                        }
                        else
                        {
                            SetProperty("gear", "-");
                        }

                        if (speed.HasValue)
                            reporter.SetSpeed((double)speed.Value);

                        if (rpm.HasValue)
                            reporter.SetRPM(rpm.Value);

                        if (!string.IsNullOrEmpty(elm.Error))
                            elm.Reset();
                    });
            }
        }

        private void UpdateWeatherForecast()
        {
            if (!hc.Config.IsInternetConnected)
                return;

            var cityId = hc.Config.GetString(ConfigNames.WeatherCityId);

            if (string.IsNullOrWhiteSpace(cityId))
                return;

            var f = weather.GetForecast(cityId);

            if (!Disposed && f != null)
            {
                int t = int.MinValue;
                int.TryParse(f.fact.First().temperature.Value, out t);
                miniDisplayModel.AirTemp = t;

                SetProperty("air_temp", string.Format("{0}°, {1}", f.fact.First().temperature.Value, f.fact.First().weather_type));

                SetProperty("weather_icon", weather.GetWeatherIcon(f.fact.First().imagev3.First().Value));
            }
            else
            {
                SetProperty("air_temp", null);
                SetProperty("weather_icon", null);
            }
        }

        private async Task UpdateAddres(GeoPoint location)
        {
			return;

            if (!Disposed && hc.Config.IsInternetConnected)
            {
                var result = await geocoder.GetAddresAsync(location);

                if (result.Success)
                    SetProperty("heading", result.Value);
                else
                    hc.Logger.Log(this, string.Concat("Update address: ", result.ErrorMessage), LogLevels.Warning);
            }
        }

        private void UpdateMiniDisplay()
        {
            miniDisplayModel.BufferedPoints = tc.BufferedPoints;
            miniDisplayModel.SendedPoints = tc.SendedPoints;
            miniDisplayModel.Draw();
        }

        protected override void OnPrimaryTick(IHostTimer timer)
        {
            if (!Disposed)
            {
                SetProperty("exported_points", string.Format("{0}/{1}", tc.BufferedPoints, tc.SendedPoints));
                SetProperty("travel_span", tc.TravelTime.TotalMinutes);
                SetProperty("distance", tc.TravelDistance);

                obdGuard.ExecuteIfFreeAsync(async () => await UpdateOBD());
                minidisplayGuard.ExecuteIfFree(UpdateMiniDisplay);
                cpuInfoGuard.ExecuteIfFreeAsync(async () => await UpdateCpuInfo(), ex => SetProperty("cpu_info", "Error"));
            }

            base.OnPrimaryTick(timer);
        }

        private async Task UpdateCpuInfo()
        {
            if (!Disposed)
            {
                var cpuSpeed = await NixHelpers.CPUInfo.GetCPUSpeed();
                var cpuTemp = await NixHelpers.CPUInfo.GetCPUTemp();

                string info = string.Format("{0} Mhz ({1}°)",
                    cpuSpeed.HasValue ? cpuSpeed.Value.ToString() : "-",
                    cpuTemp.HasValue ? cpuTemp.Value.ToString("0.0") : "-");

                if (!Disposed)
                    SetProperty("cpu_info", info);
            }
        }

        protected override void OnDisposing(object sender, EventArgs e)
        {
            focusedItemIndex = SelectedIndexAbsolute;

            weatherGuard.Dispose();
            geocoderGuard.Dispose();

            gpsController.GPRMCReseived -= GPRMCReseived;
            base.OnDisposing(sender, e);
        }

        private async void GPRMCReseived(GPRMC gprmc)
        {
            if (!Disposed)
            {
                SetProperty("speed", gprmc.Active ? gprmc.Speed.ToString("0") : "-");
                SetProperty("location", gprmc.Location);

                if (gprmc.Active)
                {
                    await geocoderGuard.ExecuteIfFreeAsync(() => UpdateAddres(gprmc.Location));
                }
            }
        }
    }
}
