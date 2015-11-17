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
            this.logger = logger;
            this.key = key;
            this.vehicle = vehicle;

            client = new HttpClient.Client();
            client.ClientException += exc => logger.Log(this, exc);
        }

        public int CreateNewLog(string body)
        {
            try
            {
                var uri = new Uri(serviceUri, string.Format("api/GeneralLog/new?key={0}&vehicle={1}", key, vehicle));
                var response = client.Post(uri, body);

                if (response.Status == System.Net.HttpStatusCode.Created)
                {
                    return int.Parse(response.Headers["LogId"]);
                }
                else
                {
                    logger.Log(this, response.Error, LogLevels.Warning);
                    return -1;
                }
            }
            catch (Exception ex)
            {
                logger.Log(this, ex);
                return -1;
            }
        }

        public async Task<int> CreateNewLogAsync(string body)
        {
            return await Task.Run<int>(() => CreateNewLog(body));
        }

        public bool AppendLog(int logId, string body)
        {
            try
            {
                var uri = new Uri(serviceUri, string.Format("api/GeneralLog/append?key={0}&vehicle={1}&id={2}", key, vehicle, logId));
                var response = client.Put(uri, body);

                return response.Status == System.Net.HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                logger.Log(this, ex);
                return false;
            }
        }

        public async Task<bool> AppendLogAsync(int logId, string body)
        {
            return await Task.Run<bool>(() => AppendLog(logId, body));
        }
    }
}
