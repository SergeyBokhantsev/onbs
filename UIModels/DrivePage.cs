using Interfaces;
using Interfaces.GPS;
using Interfaces.Input;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YandexServicesProvider;

namespace UIModels
{
    public class DrivePage : CommonPageBase
    {
        private readonly ITravelController tc;

        private readonly WeatherProvider weather;
        private readonly GeocodingProvider geocoder;
        private bool weatherProviderBusy;

        private DateTime lastGeocodeCall;

        public DrivePage(IHostController hc)
            :base(hc, "DrivePage")
        {
            this.tc = hc.GetController<ITravelController>();

            this.weather = new WeatherProvider(hc.Logger);
            this.geocoder = new GeocodingProvider(hc.Logger);

            hc.GetController<IGPSController>().GPRMCReseived += GPRMCReseived;
        }

        protected override void OnSecondaryTimer(object sender, EventArgs e)
        {
            if (!Disposed && !weatherProviderBusy && hc.Config.IsInternetConnected)
            {
                weatherProviderBusy = true;
                weather.GetForecastAsync(hc.Config.GetString(ConfigNames.WeatherCityId), OnWeatherForecast);
            }

            base.OnSecondaryTimer(sender, e);
        }

        private void OnWeatherForecast(forecast f)
        {
            if (!Disposed)
            {
                SetProperty("air_temp", f != null
                    ? string.Format("{0}°, {1}", f.fact.First().temperature.Value, f.fact.First().weather_type)
                    : "--");
            }

            weatherProviderBusy = false;
        }

        protected override void OnPrimaryTick(object sender, EventArgs e)
        {
            if (!Disposed)
            {
                SetProperty("exported_points", string.Format("{0}/{1}", tc.BufferedPoints, tc.SendedPoints));
                SetProperty("travel_span", tc.TravelTime.TotalMinutes);
                SetProperty("distance", tc.TravelDistance);
            }

            base.OnPrimaryTick(sender, e);
        }

        protected override void OnDisposing(object sender, EventArgs e)
        {
            hc.GetController<IGPSController>().GPRMCReseived -= GPRMCReseived;
            base.OnDisposing(sender, e);
        }

        void GPRMCReseived(GPRMC gprmc)
        {
            if (!Disposed)
            {
                SetProperty("gps_status", gprmc.Active);
                SetProperty("speed", gprmc.Active ? gprmc.Speed : 0);
                SetProperty("location", gprmc.Location);

                if (gprmc.Active && (DateTime.Now - lastGeocodeCall).Seconds > 3)
                {
                    geocoder.GetAddresAsync(gprmc.Location, addres => SetProperty("heading", addres));
                    lastGeocodeCall = DateTime.Now;
                }
            }
        }
    }
}
