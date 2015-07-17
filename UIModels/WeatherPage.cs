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
using System.Reflection;
using System.IO;

namespace UIModels
{
    public class WeatherPage : CommonPageBase
    {
        private readonly string iconPath;

        private readonly WeatherProvider weather;
        private bool weatherProviderBusy;

        public WeatherPage(IHostController hc)
            :base(hc, "WeatherPage")
        {
            this.weather = new WeatherProvider(hc.Logger);

            this.iconPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data\\Weather");
        }

        protected override void OnSecondaryTimer(object sender, EventArgs e)
        {
            if (!weatherProviderBusy && hc.Config.IsInternetConnected)
            {
                weatherProviderBusy = true;
                weather.GetForecastAsync(hc.Config.GetString(ConfigNames.WeatherCityId), OnWeatherForecast);
            }

            base.OnSecondaryTimer(sender, e);
        }

        private void OnWeatherForecast(forecast f)
        {
            if (f != null)
            {
                var fact = f.fact.First();

                SetProperty("now_icon_path", GetWeatherIcon(fact.imagev3.First().Value));
                SetProperty("now_condition", fact.weather_type);
                SetProperty("now_temp", fact.temperature.Value);

                var today = f.day[0];

                var eveningPart = today.day_part.First(dp => dp.type == "evening");
                SetProperty("next1_caption", "Вечером");
                SetProperty("next1_icon_path", GetWeatherIcon(eveningPart.imagev3.First().Value));
                SetProperty("next1_condition", eveningPart.weather_type);
                SetProperty("next1_temp", string.Format("{0} - {1}", eveningPart.temperature_from, eveningPart.temperature_to));

                var nightPart = today.day_part.First(dp => dp.type == "night");
                SetProperty("next2_caption", "Ночью");
                SetProperty("next2_icon_path", GetWeatherIcon(nightPart.imagev3.First().Value));
                SetProperty("next2_condition", nightPart.weather_type);
                SetProperty("next2_temp", string.Format("{0} - {1}", nightPart.temperature_from, nightPart.temperature_to));

                SetProperty("info1", string.Format("Восход: {0} Закат: {1}", today.sunrise, today.sunset));
                SetProperty("info2", string.Format("Ветер: {0} м/с, {1}", fact.wind_speed, fact.wind_direction));
                SetProperty("info3", string.Format("Влажность: {0}%", fact.humidity));
                SetProperty("info4", string.Format("Давление: {0} мм рт. ст.", fact.pressure.First().Value));
                SetProperty("info5", string.Format("Данные на {0}", DateTime.Parse(fact.observation_time).ToString("HH:mm")));
            }

            weatherProviderBusy = false;
        }

        private string GetWeatherIcon(string code)
        {
            return Path.Combine(iconPath, string.Concat(code, ".png"));
        }
    }
}
