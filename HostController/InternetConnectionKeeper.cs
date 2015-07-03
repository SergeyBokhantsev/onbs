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

        private IProcessRunner dialer;

        private bool disposed;

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

        private void CheckerThread()
        {
            Thread.Sleep(config.GetInt(ConfigNames.InetKeeperCheckInitialDelayMs));

            while (!disposed)
            {
                if (!GetConnected())
                {
                    OnInternetConnectionStatus(false);
                    Connect();
                }
                else
                {
                    OnInternetConnectionStatus(true);
                }

                DumpDiallerOutput();

                Thread.Sleep(30000);
            }
        }

        [Conditional("DEBUG")]
        private void DumpDiallerOutput()
        {
            if (dialer != null)
            {
                var dump = dialer.GetFromStandardOutput();

                if (!string.IsNullOrEmpty(dump))
                {
                    logger.Log(this, "DIALER OUTPUT DUMP:", LogLevels.Info);
                    logger.Log(this, dump, LogLevels.Info);
                }
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
            // Release carrier
            if (dialer != null && !dialer.HasExited)
            {
                dialer.Exit();
                Thread.Sleep(3000);
            }

            var modeSwitchApp = config.GetString(ConfigNames.InetKeeperUSBModeswitchApp);
            var modeswitch = runnerFactory.Create("sudo", modeSwitchApp, false);
            modeswitch.Run();

            Thread.Sleep(5000);
            logger.LogIfDebug(this, "MODESWITCH OUTPUT DUMP:");
            logger.LogIfDebug(this, modeswitch.GetFromStandardOutput());

            modeswitch.Exit();

            var diallerApp = config.GetString(ConfigNames.InetKeeperDialerApp);
            dialer = runnerFactory.Create("sudo", diallerApp, false);
            dialer.Run();

            Thread.Sleep(10000);
            logger.LogIfDebug(this, "DIALER OUTPUT DUMP:");
            logger.LogIfDebug(this, dialer.GetFromStandardOutput());
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
                            OnInternetTime(dateTime);
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
