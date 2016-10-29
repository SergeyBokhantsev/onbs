using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HttpServiceNamespace;
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

    public class MapResultReader : IResultReader<SimpleResult<Stream>>
    {
        public async Task<SimpleResult<Stream>> FromResponse(HttpStatusCode statusCode, WebHeaderCollection headers, Stream responseStream)
        {
            if (statusCode == HttpStatusCode.OK)
            {
                var ret = await responseStream.ToMemoryStreamAsync();
                return new SimpleResult<Stream>(true, statusCode, null, ret);
            }
            else
                throw new Exception(string.Concat("Unecpected HTTP code ", statusCode));
        }

        public SimpleResult<Stream> FromException(string errorMessage, HttpStatusCode httpCode)
        {
            return new SimpleResult<Stream>(false, httpCode, errorMessage, null);
        }
    }

    public class StaticMapProvider
    {
        private readonly HttpService client = new HttpService(null, null) { HttpGetTimeout = new TimeSpan(0, 0, 10), HttpPostTimeout = new TimeSpan(0, 0, 10) };

        public async Task<SimpleResult<Stream>> GetMapAsync(GeoPoint center, int width, int height, int zoom, MapLayers layers)
        {
            width = Math.Min(width, 600);
            height = Math.Min(height, 450);

            var l = string.Join(",", Enum.GetValues(typeof(MapLayers)).Cast<Enum>().Where(layers.HasFlag).Select(f => f.ToString()));

            var uri = new Uri(string.Format("https://static-maps.yandex.ru/1.x/?ll={0},{1}&z={2}&l={3}&size={4},{5}", center.Lon.Degrees, center.Lat.Degrees, zoom, l, width, height));

            var args = ExecuteArguments<SimpleResult<Stream>>.CreateGET(uri, new MapResultReader());

            return await client.ExecuteAsync(args);
        }
    }
}
