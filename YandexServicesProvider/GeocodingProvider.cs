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
    public class AddressResultReader : IResultReader<SimpleResult<string>>
    {
        public async Task<SimpleResult<string>> FromResponse(HttpStatusCode statusCode, WebHeaderCollection headers, Stream responseStream)
        {
            if (statusCode == HttpStatusCode.OK)
            {
                var ms = await responseStream.ToMemoryStreamAsync();
                var parser = new Json.JsonParser();
                var obj = parser.Parse(ms);
                var addres = Json.JPath.GetFieldValue<string>(obj, "response.GeoObjectCollection.featureMember.[0].GeoObject.metaDataProperty.GeocoderMetaData.text");

                return new SimpleResult<string>(true, statusCode, null, addres);
            }
            else
                throw new Exception(string.Concat("Unecpected HTTP code ", statusCode));
        }

        public SimpleResult<string> FromException(string errorMessage, HttpStatusCode httpCode)
        {
            return new SimpleResult<string>(false, httpCode, errorMessage, null);
        }
    }

    public class GeocodingProvider
    {
        private readonly HttpService client = new HttpService(null, null) { HttpGetTimeout = new TimeSpan(0, 0, 10), HttpPostTimeout = new TimeSpan(0, 0, 10) };

        public async Task<SimpleResult<string>> GetAddresAsync(GeoPoint location)
        {
            var uri = new Uri(string.Format("https://geocode-maps.yandex.ru/1.x/?geocode={0},{1}&results=1&format=json", location.Lon.Degrees, location.Lat.Degrees));

            var args = ExecuteArguments<SimpleResult<string>>.CreateGET(uri, new AddressResultReader());

            return await client.ExecuteAsync(args);
        }
    }
}
