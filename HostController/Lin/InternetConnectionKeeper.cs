using System;
using System.Linq;
using System.Net;
using System.Threading;
using Interfaces;
using System.IO;
using ProcessRunnerNamespace;
using System.Diagnostics;

//$ ls -al /sys/class/tty/ttyUSB0//device/driver
//lrwxrwxrwx 1 root root 0 sep  6 21:28 /sys/class/tty/ttyUSB0//device/driver -> ../../../bus/platform/drivers/usbseria

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

        public InternetConnectionKeeper(IConfig config, ILogger logger)
        {
            this.config = Ensure.ArgumentIsNotNull(config);
            this.logger = Ensure.ArgumentIsNotNull(logger);

            modemSwitcher = new ModemSwitcher(logger, config);
            dialer = new Dialer(logger, config);

            checkFolder = config.GetString(ConfigNames.InetKeeperCheckFolder);

            if (string.IsNullOrWhiteSpace(checkFolder))
                // ReSharper disable once NotResolvedInText
                throw new ArgumentNullException("InetKeeperCheckFolder is not configured");
        }

        public void StartChecking()
        {
            if (config.GetBool(ConfigNames.InetKeeperEnabled) && config.Environment == Environments.RPi)
            {
                var thread = new Thread(CheckerThread)
                {
                    Name = "Inet Keeper",
                    IsBackground = true,
                    Priority = ThreadPriority.Lowest
                };

                thread.Start();
            }
            else
                OnInternetConnectionStatus(true);
        }

        private void CheckerThread()
        {
            int generalRetries = 0;

            while (!disposed)
            {
                var connected = GetConnected();
                OnInternetConnectionStatus(connected);

                if (!connected)
                {
                    logger.LogIfDebug(this, "Begin inet connection procedures...");

                    generalRetries++;

                    if (generalRetries > 2)
                    {
                        logger.Log(this, "Still no inet connection. Trying to find and reset modem...", LogLevels.Warning);
                        modemSwitcher.CheckAndReset();
                        generalRetries = 0;
                    }

                    if (modemSwitcher.CheckAndSwitch())
                    {
						var dialerProcess = dialer.CheckAndRun();

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
									break;
								}

								if (GetConnected ())
								{
									logger.Log(this, "Inet connection confirmed.", LogLevels.Info);
                                    OnInternetConnectionStatus(true);
									success = true;
                                    generalRetries = 0;
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
                        logger.Log(this, "No modem found or modem switch procedure failed", LogLevels.Warning);
                    }
                }
                else
                {
                    generalRetries = 0;
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
				var request = WebRequest.Create(config.GetString(ConfigNames.InetKeeperCheckUrl)) as HttpWebRequest;

                if (request == null)
                    throw new Exception("Request is null");

                request.Method = config.GetString(ConfigNames.InetKeeperCheckMethod);
                request.UserAgent = config.GetString(ConfigNames.InetKeeperCheckUserAgent);

                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response == null)
                        return false;

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

        public ModemSwitcher(ILogger logger, IConfig config)
        {
            this.logger = Ensure.ArgumentIsNotNull(logger);
            this.config = Ensure.ArgumentIsNotNull(config);
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

        internal void CheckAndReset()
        {
            var mode = GetModemMode();

            switch (mode)
            {
                case ModemModes.Modem:
                case ModemModes.Storage:
                    ResetDevice_new(mode);
                    break;

                default:
                    logger.Log(this, "No modem found, nothing to reset", LogLevels.Warning);
                    break;
            }
        }

        private void ResetDevice_new(ModemModes mode)
        {
            var deviceVid = config.GetString(ConfigNames.Modem_vid);
            string devicePid;

            switch (mode)
            {
                case ModemModes.Modem:
                    devicePid = config.GetString(ConfigNames.Modem_modemmode_pid);
                    break;

                case ModemModes.Storage:
                    devicePid = config.GetString(ConfigNames.Modem_storagemode_pid);
                    break;

                default:
                    return;
            }

            var modemDevice = NixHelpers.LsUsb.EnumerateDevices().FirstOrDefault(d => d.VID == deviceVid && d.PID == devicePid);

            if (null == modemDevice)
            {
                logger.Log(this, string.Format("Unable to reset modem. No corresponding device found ({0}:{1})", deviceVid, devicePid), LogLevels.Error);
                return;
            }

            ProcessRunner pr = null;

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "sudo",
                    Arguments = Path.Combine(config.DataFolder, string.Format("usbreset /dev/bus/usb/{0}/{1}", modemDevice.Bus, modemDevice.Device)),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                pr = new ProcessRunner(psi, true, true);

                pr.Run();

                if (pr.WaitForExit(30000))
                {
                    string outStr = null;

                    pr.ReadStdOut(ms => outStr = ms.GetString());

                    if (null != outStr)
                        logger.Log(this, string.Format("Device {0}:{1} seems was reset successfully: {2}", modemDevice.PID, modemDevice.VID, outStr), LogLevels.Info);
                    else
                        logger.Log(this, string.Format("Device {0}:{1} seems was not reset successfully", modemDevice.PID, modemDevice.VID), LogLevels.Warning);
                }
                else
                {
                    logger.Log(this, string.Format("Timeout resetting Device {0}:{1}.", modemDevice.VID, modemDevice.PID), LogLevels.Warning);
                }
            }
            catch (Exception ex)
            {
                logger.Log(this, ex);
            }
            finally
            {
                if (null != pr && !pr.HasExited)
                    pr.Exit();
            }
        }

        private void ResetDevice(ModemModes mode)
        {
            var deviceVid = config.GetString(ConfigNames.Modem_vid);
            string devicePid;

            switch (mode)
            {
                case ModemModes.Modem:
                    devicePid = config.GetString(ConfigNames.Modem_modemmode_pid);
                    break;

                case ModemModes.Storage:
                    devicePid = config.GetString(ConfigNames.Modem_storagemode_pid);
                    break;

                default:
                    return;
            }

            ProcessRunner pr = null;

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = config.GetString("modeswitch_reset_exe"),
                    Arguments = string.Format(config.GetString("modeswitch_reset_args"), deviceVid, devicePid),
                    UseShellExecute = false
                };

                pr = new ProcessRunner(psi, false, false);
                pr.Run();

                if (pr.WaitForExit(60000))
                {
                    logger.Log(this, "Device {0}:{1} was reset successfully", LogLevels.Info);
                }
                else
                {
                    logger.Log(this, "There was some truoble resetting Device {0}:{1}. The Reset process still not finished after 60 seconds.", LogLevels.Warning);
                }
            }
            catch (Exception ex)
            {
                logger.Log(this, ex);
            }
            finally
            {
                if (null != pr && !pr.HasExited)
                    pr.Exit();
            }
        }

        private bool SwitchToModem()
        {
			ProcessRunner pr = null;

            try
            {
                var modemSwitchConfigFilePath = Path.Combine(config.DataFolder, "12d1_1446.cfg");

                var psi = new ProcessStartInfo
                {
                    FileName = config.GetString("modeswitch_exe"),
                    Arguments = string.Format(config.GetString("modeswitch_args"), modemSwitchConfigFilePath),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                pr = new ProcessRunner(psi, true, true);
                pr.Run();

                if (!pr.WaitForExit(30000))
                {
                    logger.Log(this, "Modemswitch tool timeout", LogLevels.Warning);
                    return false;
                }

                string output = null;

                if (!pr.ReadStdOut(ms => output = ms.GetString()))
                {
                    logger.Log(this, "Modeswitch tool return no data", LogLevels.Warning);

                    if (pr.ReadStdError(ms => output = ms.GetString()) && !string.IsNullOrWhiteSpace(output))
                    {
                        logger.Log(this, output, LogLevels.Warning);
                    }
                    
                    return false;
                }

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
			finally
			{
				if (null != pr && !pr.HasExited)
					pr.Exit ();
			}
        }

        private ModemModes GetModemMode()
        {
            try
            {
                var modemVid = config.GetString(ConfigNames.Modem_vid);
                var modemPid_modemMode = config.GetString(ConfigNames.Modem_modemmode_pid);
                var modemPid_storageMode = config.GetString(ConfigNames.Modem_storagemode_pid);

                //foreach (var dev in NixHelpers.DmesgFinder.EnumerateUSBDevices(prf))
                foreach (var dev in NixHelpers.LsUsb.EnumerateDevices())
                {
                    if (dev.VID == modemVid)
                    {
                        if (modemPid_modemMode.Equals(dev.PID, StringComparison.InvariantCultureIgnoreCase))
                            return ModemModes.Modem;

                        if (modemPid_storageMode.Equals(dev.PID, StringComparison.InvariantCultureIgnoreCase))
                            return ModemModes.Storage;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(this, ex);                
            }

            return ModemModes.NotFound;
        }
    }

    internal class Dialer
    {
        private readonly ILogger logger;
        private readonly IConfig config;

        private NixHelpers.USBDevice modemDevice;

        public Dialer(ILogger logger, IConfig config)
        {
            this.logger = Ensure.ArgumentIsNotNull(logger);
            this.config = Ensure.ArgumentIsNotNull(config);
        }

        public ProcessRunner CheckAndRun()
        {
            try
            {
                KillRunningDialers();
                return RunDialer();
            }
            catch (Exception ex)
            {
                logger.Log(this, ex);
                modemDevice = null;
				return null;
            }
        }

        private ProcessRunner RunDialer()
        {
            if (null == modemDevice)
            {
                logger.Log(this, "Resolving Modem device via dmesg", LogLevels.Info);

                var modemVid = config.GetString(ConfigNames.Modem_vid);
                var modemPid_modemMode = config.GetString(ConfigNames.Modem_modemmode_pid);
                modemDevice = NixHelpers.DmesgFinder.FindUSBDevice(modemVid, modemPid_modemMode);

                if (modemDevice == null)
                    throw new Exception(string.Format("No {0}:{1} USB device found", modemVid, modemPid_modemMode));

                logger.Log(this, string.Format("Modem device resolved: {0}:{1}", modemDevice.VID, modemDevice.PID), LogLevels.Info);
            }

            if (modemDevice.AttachedTo == null || !modemDevice.AttachedTo.Any())
                throw new Exception(string.Format("USB device {0}:{1} has no any ttyUSB attached", modemDevice.VID, modemDevice.PID));

			var attached = modemDevice.AttachedTo.ToArray ();

			var usbPort = attached.Length > 2 ? attached[attached.Length - 3] : attached.First();

            if (string.IsNullOrWhiteSpace(usbPort) || !usbPort.Contains("ttyUSB"))
                throw new Exception(string.Format("USB device {0}:{1} has invalid attached ttyUSB: {2}", modemDevice.VID, modemDevice.PID, usbPort));

			var dialConfigTemplatePath = Path.Combine (config.DataFolder, "wvdial.conf");
			var dialConfig = File.ReadAllText (dialConfigTemplatePath);
			dialConfig = string.Format (dialConfig, usbPort);

			var dialConfigPath = Path.Combine (config.DataFolder, "_wd.conf");
			File.WriteAllText (dialConfigPath, dialConfig);

            logger.Log(this, string.Format("Running Dialer for {0}, config file created in {1}", usbPort, dialConfigPath), LogLevels.Info);

            var psi = new ProcessStartInfo
            {
                FileName = config.GetString("dialer_exe"),
                Arguments = string.Format(config.GetString("dialer_args"), dialConfigPath),
                UseShellExecute = false
            };

            var dialer = new ProcessRunner(psi, false, false);
            dialer.Run();

			if (dialer.WaitForExit(10000))
            {
                logger.Log(this, "Inet dealer exited too fast...", LogLevels.Warning);
                return null;
            }
            else
                return dialer;
        }

        private void KillRunningDialers()
        {
			var dialerAppName = config.GetString("dialer_exe");

			int dialerPID;
            int killTries = 0;

			while ((dialerPID = NixHelpers.ProcessFinder.FindProcess(dialerAppName)) !=-1)
            {
                if (++killTries > 10)
                {
                    logger.Log(this, string.Format("Cannot kill running dealer PID {0}", dialerPID), LogLevels.Error);
                    return;
                }

                logger.Log(this, string.Format("Another dialler instance found, pid {0}, trying to kill...", dialerPID), LogLevels.Info);

				var result = ProcessRunner.ExecuteTool("kill dealers", (string str) => str, 2000, "sudo", "kill " + dialerPID.ToString());

				if (!string.IsNullOrEmpty(result))
					logger.Log(this, string.Concat("Kill dialer unexpected output: ", result), LogLevels.Warning);

				Thread.Sleep (1000);
            }
        }
    }
}
