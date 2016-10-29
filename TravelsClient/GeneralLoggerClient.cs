using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HttpServiceNamespace;
using Interfaces;

namespace TravelsClient
{
    public class CreateLogResultReader : IResultReader<SimpleResult<int>>
    {
        public Task<SimpleResult<int>> FromResponse(HttpStatusCode statusCode, WebHeaderCollection headers, Stream responseStream)
        {
            return Task.FromResult(new SimpleResult<int>(true, statusCode, null, int.Parse(headers["LogId"])));
        }

        public SimpleResult<int> FromException(string errorMessage, HttpStatusCode httpCode)
        {
            return new SimpleResult<int>(false, httpCode, errorMessage, -1);
        }
    }

    public class AppendLogResultReader : IResultReader<SimpleResult>
    {
        public Task<SimpleResult> FromResponse(HttpStatusCode statusCode, WebHeaderCollection headers, Stream responseStream)
        {
            if (statusCode != HttpStatusCode.OK)
                throw new Exception(string.Format("Http Operation code is {0} but 200 (OK) expected", statusCode));

            return Task.FromResult(new SimpleResult(true, statusCode, null));
        }

        public SimpleResult FromException(string errorMessage, HttpStatusCode httpCode)
        {
            return new SimpleResult(false, httpCode, errorMessage);
        }
    }

    public class GeneralLoggerClient
    {
        private readonly Uri serviceUri;
        private readonly HttpService client;
        private readonly string key;
        private readonly string vehicle;

        public GeneralLoggerClient(Uri serviceUri, string key, string vehicle)
        {
            this.serviceUri = serviceUri;
            this.key = key;
            this.vehicle = vehicle;

            client = new HttpService(null, null)
            {
                HttpGetTimeout = new TimeSpan(0, 0, 30),
                HttpPostTimeout = new TimeSpan(0, 0, 45)
            };
        }

        public async Task<SimpleResult<int>> CreateNewLogAsync(string body)
        {
            var uri = new Uri(serviceUri, string.Format("api/GeneralLog/new?key={0}&vehicle={1}", key, vehicle));

            var args = ExecuteArguments<SimpleResult<int>>.Create(uri, "POST", new CreateLogResultReader(), new StringRequestWriter(body));

            return await client.ExecuteAsync(args);
        }

        public async Task<SimpleResult> AppendLogAsync(int logId, string body)
        {
            var uri = new Uri(serviceUri, string.Format("api/GeneralLog/append?key={0}&vehicle={1}&id={2}", key, vehicle, logId));

            var args = ExecuteArguments<SimpleResult>.Create(uri, "PUT", new AppendLogResultReader(), new StringRequestWriter(body));

            return await client.ExecuteAsync(args);
        }
    }
}
