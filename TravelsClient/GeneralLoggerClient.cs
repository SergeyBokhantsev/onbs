using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace TravelsClient
{
    public class GeneralLoggerClient
    {
        private readonly Uri serviceUri;
        private readonly HttpClient.Client client;
        private readonly string key;
        private readonly string vehicle;

        public GeneralLoggerClient(Uri serviceUri, string key, string vehicle)
        {
            this.serviceUri = serviceUri;
            this.key = key;
            this.vehicle = vehicle;

            client = new HttpClient.Client();
        }

        public int CreateNewLog(string body)
        {
            var uri = new Uri(serviceUri, string.Format("api/GeneralLog/new?key={0}&vehicle={1}", key, vehicle));

            using (var response = client.Post(uri, body, 2, 3000))
            {
                if (response.Status == System.Net.HttpStatusCode.Created)
                {
                    return int.Parse(response.Headers["LogId"]);
                }
                else
                {
                    throw new Exception(string.Format("Unable to create new log: {0}", response.Error));
                }
            }
        }

        public async Task<int> CreateNewLogAsync(string body)
        {
            return await Task.Run<int>(() => CreateNewLog(body));
        }

        public void AppendLog(int logId, string body)
        {
            var uri = new Uri(serviceUri, string.Format("api/GeneralLog/append?key={0}&vehicle={1}&id={2}", key, vehicle, logId));
            using (var response = client.Put(uri, body, 3, 3000))
            {
                if (response.Status != System.Net.HttpStatusCode.OK)
                    throw new Exception(string.Format("Unable to append log: {0}", response.Error));
            }
        }

        public async Task AppendLogAsync(int logId, string body)
        {
            await Task.Run(() => AppendLog(logId, body));
        }
    }
}
