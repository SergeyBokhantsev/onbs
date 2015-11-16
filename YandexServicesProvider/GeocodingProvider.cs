using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.GPS;

namespace YandexServicesProvider
{
    public class GeocodingProvider
    {
        private readonly ILogger logger;
        private readonly HttpClient.Client client = new HttpClient.Client();

        public GeocodingProvider(ILogger logger)
        {
            this.logger = logger;
        }

        public string GetAddres(GeoPoint location)
        {
            string url = string.Format("https://geocode-maps.yandex.ru/1.x/?geocode={0},{1}&results=1&format=json", location.Lon.Degrees, location.Lat.Degrees);

            try
            {
                using (var responce = client.Get(new Uri(url)))
                {
                    if (responce.Status == System.Net.HttpStatusCode.OK)
                    {
                        using (var inputStream = responce.GetStream())
                        {
                            var parser = new Json.JsonParser();
                            var obj = parser.Parse(inputStream);

                            var addres = Json.JPath.GetFieldValue<string>(obj, "response.GeoObjectCollection.featureMember.[0].GeoObject.metaDataProperty.GeocoderMetaData.text");

                            return addres;
                        }
                    }
                    else
                    {
                        logger.Log(this, string.Format("Unable to get addres: {0}", responce.Error), LogLevels.Warning);
                        return null;
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Log(this, ex);
                return null;
            }
        }

        public void GetAddresAsync(GeoPoint location, Action<string> callback)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                var addres = GetAddres(location);

                if (callback != null)
                    callback(addres);
            });
        }

        public async Task<string> GetAddresAsync(GeoPoint location)
        {
            return await Task.Run<string>(() => GetAddres(location));
        }
    }
}
