using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interfaces;

namespace HostController
{
    internal class InternetConnectionKeeper : IDisposable
    {
        public event Action<bool> InternetConnectionStatus;
        public event Action<DateTime> InternetTime;

        private readonly IProcessRunnerFactory runnerFactory;
        private readonly IConfig config;
        private readonly ILogger logger;

        private bool initialDelayExecuted;
        private bool disposed;
        private bool connected;

        public InternetConnectionKeeper(IProcessRunnerFactory runnerFactory, IConfig config, ILogger logger)
        {
            this.runnerFactory = runnerFactory;
            this.config = config;
            this.logger = logger;
        }

        public void StartChecking()
        {
            var thread = new Thread(CheckerThread);
            thread.Name = "Inet Keeper";
            thread.IsBackground = true;
            thread.Priority = ThreadPriority.Lowest;
            thread.Start();
        }

        public void WaitForConnection(int timeoutMs)
        {
            const int span = 200;
            int waited = 0;

            while (!connected && waited < timeoutMs)
            {
                Thread.Sleep(span);
                waited += span;
            }

			OnInternetConnectionStatus (connected);
        }

        private void CheckerThread()
        {
            while (!disposed)
            {
                if (!GetConnected())
                {
                    OnInternetConnectionStatus(connected = false);

                    if (!initialDelayExecuted)
                    {
                        Thread.Sleep(config.GetInt(ConfigNames.InetKeeperCheckInitialDelayMs));
                        initialDelayExecuted = true;
                    }

                    Connect();
                }
                else
                {
                    initialDelayExecuted = true;
                    OnInternetConnectionStatus(connected = true);
                }

                Wait(30000);
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

        private void Connect()
        {
            try
            {
                var switcherCommand = config.GetString(ConfigNames.InetKeeperModeswitchCommand);
                var switcher = runnerFactory.Create("sudo", switcherCommand, false);
                switcher.Run();
                switcher.WaitForExit(10000);
                logger.LogIfDebug(this, switcher.GetFromStandardOutput());

                var dialCommand = config.GetString(ConfigNames.InetKeeperDialCommand);
                var dialler = runnerFactory.Create("sudo", dialCommand, false);
                dialler.Run();
                dialler.WaitForExit(5000);
            }
            catch (Exception ex)
            {
                logger.Log(this, ex);
            }
        }

        private bool GetConnected()
        {
            try
            {
                var request = WebRequest.Create(config.GetString
				                                (ConfigNames.InetKeeperCheckUrl)) as HttpWebRequest;
                request.Method = config.GetString(ConfigNames.InetKeeperCheckMethod);
                var response = request.GetResponse() as HttpWebResponse;

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
