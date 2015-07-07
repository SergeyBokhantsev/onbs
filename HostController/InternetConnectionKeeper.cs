using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interfaces;
using System.IO;

namespace HostController
{
    internal class InternetConnectionKeeper : IDisposable
    {
        public event Action<bool> InternetConnectionStatus;
        public event Action<DateTime> InternetTime;

        private readonly IConfig config;
        private readonly ILogger logger;
        private readonly string checkFolder;

        private bool disposed;

        public InternetConnectionKeeper(IConfig config, ILogger logger)
        {
            this.config = config;
            this.logger = logger;

            checkFolder = config.GetString(ConfigNames.InetKeeperCheckFolder);

            if (string.IsNullOrWhiteSpace(checkFolder))
                throw new ArgumentNullException("ConfigNames.InetKeeperCheckFolder");
        }

        public void StartChecking()
        {
            if (config.GetBool(ConfigNames.InetKeeperEnabled))
            {
                var thread = new Thread(CheckerThread);
                thread.Name = "Inet Keeper";
                thread.IsBackground = true;
                thread.Priority = ThreadPriority.Lowest;
                thread.Start();
            }
            else
                OnInternetConnectionStatus(true);
        }

        private void CheckerThread()
        {
            while (!disposed)
            {
                OnInternetConnectionStatus(GetConnected());
                Wait(10000);
            }
        }

		private void Wait(int timeout)
		{
			const int span = 2000;
			int waited = 0;

			while (!disposed && waited < timeout) 
			{
				Thread.Sleep (span);
				waited += span;
			}
		}

        private void OnInternetConnectionStatus(bool connected)
        {
            var handler = InternetConnectionStatus;
            if (handler != null)
                handler(connected);
        }

        private void OnInternetTime(DateTime time)
        {
            var handler = InternetTime;
            if (handler != null)
                handler(time);
        }

        private bool GetConnected()
        {
            try
            {
				if (!Directory.Exists(checkFolder))
					return false;

				if (InternetTime != null)
				{
	                var request = WebRequest.Create(config.GetString
					                                (ConfigNames.InetKeeperCheckUrl)) as HttpWebRequest;
	                request.Method = config.GetString(ConfigNames.InetKeeperCheckMethod);

	                using (var response = request.GetResponse() as HttpWebResponse)
	                {
	                    if (response.StatusCode == HttpStatusCode.OK)
	                    {
	                        if (InternetTime != null)
	                        {
	                            try
	                            {
	                                var date = response.Headers[HttpResponseHeader.Date];
	                                var dateTime = DateTime.Parse(date);
	                                OnInternetTime(dateTime.ToUniversalTime());
	                            }
	                            catch (Exception ex)
	                            {
	                                logger.Log(this, "Exception trying to get internet datetime.", LogLevels.Warning);
	                                logger.Log(this, ex);
	                            }
	                        }

	                        return true;
	                    }
	                    else
	                    {
	                        return false;
	                    }
	                }
				}
				else 
					return true;
			}
            catch (WebException)
            {
                return false;
            }
			catch (Exception ex) 
			{
				logger.Log (this, ex);
				return false;
			}
        }

        public void Dispose()
        {
            disposed = true;
        }
    }
}
