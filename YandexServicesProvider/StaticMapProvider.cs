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
    [Flags]
    public enum MapLayers 
    {
        /// <summary>
        /// Схема местности и названия географических объектов
        /// </summary>
        map = 1,
        /// <summary>
        /// Слой Народной карты
        /// </summary>
        pmap = 2,
        /// <summary>
        /// Местность, сфотографированная со спутника
        /// </summary>
        sat = 4,
        /// <summary>
        /// Названия географических объектов
        /// </summary>
        skl = 8,
        /// <summary>
        /// Названия географических объектов Народной карты
        /// </summary>
        pskl = 16,
        /// <summary>
        /// Слой пробок
        /// </summary>
        trf = 32
    }

    public class StaticMapProvider
    {
        private readonly ILogger logger;
        private readonly HttpClient.Client client = new HttpClient.Client();

        public StaticMapProvider(ILogger logger)
        {
            this.logger = logger;
        }

        public Stream GetMap(GeoPoint center, int width, int height, int zoom, MapLayers layers)
        {
            width = Math.Min(width, 600);
            height = Math.Min(height, 450);

            var l = string.Join(",", Enum.GetValues(typeof(MapLayers)).Cast<Enum>().Where(layers.HasFlag).Select(f => f.ToString()));

            string url = string.Format("https://static-maps.yandex.ru/1.x/?ll={0},{1}&z={2}&l={3}&size={4},{5}", center.Lon.Degrees, center.Lat.Degrees, zoom, l, width, height);

            try
            {
                using (var responce = client.Get(new Uri(url)))
                {
                    if (responce.Status == System.Net.HttpStatusCode.OK)
                    {
                        var ret = new MemoryStream();

                        using (var inputStream = responce.GetStream())
                        {
                            var buffer = new byte[2048];
                            int readed = 0;
                            do
                            {
                                readed = inputStream.Read(buffer, 0, buffer.Length);
                                ret.Write(buffer, 0, readed);
                            }
                            while (readed > 0);
                        }

                        return ret;
                    }
                    else
                    {
                        logger.Log(this, string.Format("Unable to get map: {0}", responce.Error), LogLevels.Warning);
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

        public void GetMapAsync(GeoPoint center, int width, int height, int zoom, MapLayers layers, Action<Stream> callback)
        {
            ThreadPool.QueueUserWorkItem(state =>
                {
                    var mapStream = GetMap(center, width, height, zoom, layers);

                    if (callback != null)
                        callback(mapStream);
                });
        }
    }
}
