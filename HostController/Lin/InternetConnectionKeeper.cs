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

namespace HostController.Lin
{
    internal class InternetConnectionKeeper : IDisposable
    {
        public event Action<bool> InternetConnectionStatus;
        public event Action<DateTime> InternetTime;
        public event Action RestartNeeded;

        private readonly IConfig config;
        private readonly ILogger logger;
        private readonly string checkFolder;

        private readonly ModemSwitcher modemSwitcher;
        private readonly Dialer dialer;

        private DateTime? inetDisconnectedAt;

        private bool disposed;

        public InternetConnectionKeeper(IConfig config, ILogger logger, IProcessRunnerFactory prf)
        {
            this.config = Ensure.ArgumentIsNotNull(config);
            this.logger = Ensure.ArgumentIsNotNull(logger);
            Ensure.ArgumentIsNotNull(prf);

            modemSwitcher = new ModemSwitcher(logger, config, prf);
            dialer = new Dialer(logger, config, prf);

            checkFolder = config.GetString(ConfigNames.InetKeeperCheckFolder);

            if (string.IsNullOrWhiteSpace(checkFolder))
                throw new ArgumentNullException("ConfigNames.InetKeeperCheckFolder");
        }

        public void StartChecking()
        {
            if (config.GetBool(ConfigNames.InetKeeperEnabled) && config.Environment == Environments.RPi)
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
                var connected = GetConnected();
                OnInternetConnectionStatus(connected);

                if (!connected)
                {
                    logger.LogIfDebug(this, "Begin inet connection procedures...");

                    if (modemSwitcher.CheckAndSwitch())
                    {
						var dialerProcess = dialer.CheckAndRun ();

						if (dialerProcess != null)
                        {
                            logger.Log(this, "Dialler started", LogLevels.Info);

							int repeat = 0;
							bool success = false;

							while (repeat++ < 10) 
							{
								if (dialerProcess.HasExited) 
								{
									logger.Log(this, "Dialer process exited unexpectedly", LogLevels.Warning);
									logger.Log(this, dialerProcess.GetFromStandardOutput(), LogLevels.Warning);
									break;
								}

								if (GetConnected ()) 
								{
									logger.Log(this, "Inet connection confirmed.", LogLevels.Info);
									success = true;
									break;
								}

								logger.Log(this, "Waiting for inet connection...", LogLevels.Info);
								Thread.Sleep (5000);
							}

							if (!success)
								continue;

							logger.Log(this, "Going to normal mode...", LogLevels.Info);
                        }
                        else
                        {
                            logger.Log(this, "Inet dialer procedure failed", LogLevels.Warning);
                        }
                    }
                    else
                    {
                        logger.Log(this, "Modem switch procedure failed", LogLevels.Warning);
                    }
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
            if (config.GetBool(ConfigNames.InetRestartIfNoConnectEnabled))
            {
                if (connected)
                {
                    inetDisconnectedAt = null;
                }
                else if (config.IsSystemTimeValid)
                {
                    if (inetDisconnectedAt.HasValue)
                    {
                        var restartMinutes = config.GetInt(ConfigNames.InetRestartIfNoConnectMinutes);
                        if ((DateTime.Now - inetDisconnectedAt.Value).Minutes >= restartMinutes)
                        {
                            OnRestartNeeded();
                        }
                    }
                    else
                    {
                        inetDisconnectedAt = DateTime.Now;
                    }
                }
            }

            var handler = InternetConnectionStatus;
            if (handler != null)
                handler(connected);
        }

        private void OnRestartNeeded()
        {
            var handler = RestartNeeded;
            if (handler != null)
                handler();
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
            catch (WebException)
            {
                return false;
            }
            catch (Exception ex)
            {
                logger.Log(this, ex);
                return false;
            }
        }

        public void Dispose()
        {
            disposed = true;
        }
    }

    internal class ModemSwitcher
    {
        //http://dmitrykhn.homedns.org/wp/2011/01/ubuntu-3g-modem-terminal/

        private enum ModemModes { Modem, Storage, NotFound };

        private readonly ILogger logger;
        private readonly IConfig config;
        private readonly IProcessRunnerFactory prf;

        public ModemSwitcher(ILogger logger, IConfig config, IProcessRunnerFactory prf)
        {
            this.logger = Ensure.ArgumentIsNotNull(logger);
            this.config = Ensure.ArgumentIsNotNull(config);
            this.prf = Ensure.ArgumentIsNotNull(prf);
        }

        public bool CheckAndSwitch()
        {
            var mode = GetModemMode();

            switch (mode)
            {
                case ModemModes.Modem:
                    return true;

                case ModemModes.Storage:
                    return SwitchToModem();
                    
                default:
                    logger.Log(this, "USB Modem device was not found in the system", LogLevels.Warning);
                    return false;
            }
        }

        private bool SwitchToModem()
        {
            try
            {
				var modemSwitchConfigFilePath = Path.Combine(config.DataFolder, "12d1:1446");
				var pr = prf.Create("modeswitch", new object[] { modemSwitchConfigFilePath });
                pr.Run();
                pr.WaitForExit(30000);
                var output = pr.GetFromStandardOutput();
                var result = output.Contains("Mode switch succeeded");

				if (!result)
					logger.Log(this, output, LogLevels.Warning);

				return result;
            }
            catch (Exception ex)
            {
                logger.Log(this, ex);
                return false;
            }
        }

        private ModemModes GetModemMode()
        {
			var modemVid = config.GetString(ConfigNames.Modem_vid);
			var modemPid_modemMode = config.GetString(ConfigNames.Modem_modemmode_pid);
			var modemPid_storageMode = config.GetString(ConfigNames.Modem_storagemode_pid);

            foreach(var dev in NixHelpers.DmesgFinder.EnumerateUSBDevices(prf))
            {
                if (dev.VID == modemVid)
                {
					if (modemPid_modemMode.Equals(dev.PID, StringComparison.InvariantCultureIgnoreCase))
                        return ModemModes.Modem;

					if (modemPid_storageMode.Equals(dev.PID, StringComparison.InvariantCultureIgnoreCase))
                        return ModemModes.Storage;
                }
            }

            return ModemModes.NotFound;
        }
    }

    internal class Dialer
    {
        private readonly ILogger logger;
        private readonly IConfig config;
        private readonly IProcessRunnerFactory prf;

        public Dialer(ILogger logger, IConfig config, IProcessRunnerFactory prf)
        {
            this.logger = Ensure.ArgumentIsNotNull(logger);
            this.config = Ensure.ArgumentIsNotNull(config);
            this.prf = Ensure.ArgumentIsNotNull(prf);
        }

        public IProcessRunner CheckAndRun()
        {
            try
            {
                KillRunningDialers();
                return RunDialer();
            }
            catch (Exception ex)
            {
                logger.Log(this, ex);
				return null;
            }
        }

        private IProcessRunner RunDialer()
        {
			var modemVid = config.GetString(ConfigNames.Modem_vid);
			var modemPid_modemMode = config.GetString(ConfigNames.Modem_modemmode_pid);
			var modemDevice = NixHelpers.DmesgFinder.FindUSBDevice (modemVid, modemPid_modemMode, prf);

			var usbPort = modemDevice.AttachedTo.First ();

			var dialConfigTemplatePath = Path.Combine (config.DataFolder, "wvdial.conf");
			var dialConfig = File.ReadAllText (dialConfigTemplatePath);
			dialConfig = string.Format (dialConfig, usbPort);

			var dialConfigPath = Path.Combine (config.DataFolder, "_wd.conf");
			File.WriteAllText (dialConfigPath, dialConfig);

			var dialer = prf.Create("dialer", new object[] { dialConfigPath });
            dialer.Run();
			dialer.WaitForExit (10000);
            return dialer;
        }

        private void KillRunningDialers()
        {
			var dialerAppName = config.GetString("dialer_exe");

			var dialerPID = -1;

			while ((dialerPID = NixHelpers.ProcessFinder.FindProcess(dialerAppName, prf)) !=-1)
            {
                var processConfig = new ProcessConfig
                {
                    ExePath = "sudo",
					Args = "kill " + dialerPID.ToString(),
                };

                prf.Create(processConfig).Run();

				Thread.Sleep (2000);
            }
        }
    }
}
