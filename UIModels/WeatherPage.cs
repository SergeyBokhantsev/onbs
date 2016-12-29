using Interfaces;
using Interfaces.UI;
using System;
using System.Linq;
using System.Threading.Tasks;
using YandexServicesProvider;

namespace UIModels
{
    public class WeatherPage : DrivePageBase
    {
        private readonly WeatherProvider weather;
        private bool weatherProviderBusy;

        public WeatherPage(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor)
        {
            this.weather = new WeatherProvider(hc.Logger, hc.Config.DataFolder);
        }

        private void UpdateWeatherForecast()
        {
            //forecast f = null;

            //if (!Disposed && !weatherProviderBusy && hc.Config.IsInternetConnected)
            //{
            //    f = await Task.Run(() =>
            //    {
            //        weatherProviderBusy = true;
            //        return weather.GetForecast(hc.Config.GetString(ConfigNames.WeatherCityId));
            //    });

            //    weatherProviderBusy = false;
            //}

            //if (f != null && !Disposed)
            //{
            //    var fact = f.fact.First();

            //    SetProperty("now_icon_path", weather.GetWeatherIcon(fact.imagev3.First().Value));
            //    SetProperty("now_condition", fact.weather_type);
            //    SetProperty("now_temp", fact.temperature.Value);

            //    var today = f.day[0];

            //    var eveningPart = today.day_part.First(dp => dp.type == "evening");
            //    SetProperty("next1_caption", "Вечером");
            //    SetProperty("next1_icon_path", weather.GetWeatherIcon(eveningPart.imagev3.First().Value));
            //    SetProperty("next1_condition", eveningPart.weather_type);
            //    SetProperty("next1_temp", string.Format("{0} - {1}", eveningPart.temperature_from, eveningPart.temperature_to));

            //    var nightPart = today.day_part.First(dp => dp.type == "night");
            //    SetProperty("next2_caption", "Ночью");
            //    SetProperty("next2_icon_path", weather.GetWeatherIcon(nightPart.imagev3.First().Value));
            //    SetProperty("next2_condition", nightPart.weather_type);
            //    SetProperty("next2_temp", string.Format("{0} - {1}", nightPart.temperature_from, nightPart.temperature_to));

            //    SetProperty("info1", string.Format("Восход: {0} Закат: {1}", today.sunrise, today.sunset));
            //    SetProperty("info2", string.Format("Ветер: {0} м/с, {1}", fact.wind_speed, fact.wind_direction));
            //    SetProperty("info3", string.Format("Влажность: {0}%", fact.humidity));
            //    SetProperty("info4", string.Format("Давление: {0} мм рт. ст.", fact.pressure.First().Value));
            //    SetProperty("info5", string.Format("Данные на {0}", DateTime.Parse(fact.observation_time).ToString("HH:mm")));

            //    for (int i = 1; i <= 6; ++i)
            //    {
            //        if (f.day.Length <= i)
            //            break;

            //        var day = f.day[i];
            //        SetProperty(string.Format("day{0}_caption", i), i == 1 ? "Завтра" : GetDayOfWeek(DateTime.Now.AddDays(i).DayOfWeek));
            //        SetProperty(string.Format("day{0}_date", i), day.date);
            //        var dPart = day.day_part.First(dp => dp.type == "day");
            //        var nPart = day.day_part.First(dp => dp.type == "night");
            //        SetProperty(string.Format("day{0}_image_path", i), weather.GetWeatherIcon(dPart.imagev3.First().Value));
            //        SetProperty(string.Format("day{0}_day_temp", i), string.Format("{0}-{1}", dPart.temperature_from, dPart.temperature_to));
            //        SetProperty(string.Format("day{0}_night_temp", i), string.Format("{0}-{1}", nPart.temperature_from, nPart.temperature_to));
            //    }
            //}
        }

        private string GetDayOfWeek(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Sunday:
                    return "Вс";
                case DayOfWeek.Monday:
                    return "Пн";
                case DayOfWeek.Tuesday:
                    return "Вт";
                case DayOfWeek.Wednesday:
                    return "Ср";
                case DayOfWeek.Thursday:
                    return "Чт";
                case DayOfWeek.Friday:
                    return "Пт";
                case DayOfWeek.Saturday:
                    return "Сб";
                default:
                    return string.Empty;
            }
        }
    }
}
