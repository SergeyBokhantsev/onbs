using Interfaces;
using NixHelpers;
using ProcessRunnerNamespace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModemConnectionKeeper
{
    public class ConnectionKeeper
    {
        private const string dialerExe = "wvdial";

        private readonly ILogger logger;
        private readonly IConfig config;
        private readonly Dialer dialer;

        private readonly string modemVid;
        private readonly string modemPid_modemMode;
        private readonly string modemPid_storageMode;

        private readonly string wvDialConfigFile;

        public ConnectionMetricsProvider Metrics
        {
            get { return dialer.Metrics; }
            set { dialer.Metrics = value; }
        }

        public ConnectionKeeper(IConfig config, ILogger logger)
        {
            if (null == logger)
                throw new ArgumentNullException("logger");

            if (null == config)
                throw new ArgumentNullException("config");

            this.logger = logger;
            this.config = config;

            modemVid = config.GetString(ConfigNames.Modem_vid);
            modemPid_modemMode = config.GetString(ConfigNames.Modem_modemmode_pid);
            modemPid_storageMode = config.GetString(ConfigNames.Modem_storagemode_pid);

            wvDialConfigFile = Path.Combine (config.DataFolder, "_wd.conf");

            dialer = new Dialer(wvDialConfigFile, logger);

            dialer.DialerProcessExited += DialerProcessExited;
		
            dialer.StateChanged += StateChanged;

            (new Thread(Dial) { IsBackground = true }).Start();
        }

        void StateChanged()
        {
			logger.Log (this, dialer.CurrentStateDescription, LogLevels.Info);
        }

        void DialerProcessExited()
        {
            logger.Log(this, "Dialer exited, restarting after 10 seconds...", LogLevels.Info);

            Thread.Sleep(10000);

            Dial();
        }

        private void ResetModem(USBBusDevice modem)
        {
            MetricMessage("ResetModem()");

            var output = ProcessRunner.ExecuteTool("Reset modem", (string o) => o,
                          15000, 
                          "sudo",
                          Path.Combine(config.DataFolder, string.Format("usbreset /dev/bus/usb/{0}/{1}", modem.Bus, modem.Device)));

            if (string.IsNullOrEmpty(output) || !output.Contains("Reset successful"))
                throw new Exception(string.Concat("Unable to reset modem: ", output));
        }

        private void MetricMessage(string message, ColoredStates state = ColoredStates.Normal)
        {
            if (null != Metrics)
                Metrics.KeeperMessage.Set(message, state);
        }

        private void Dial()
        {
            try
            {
				logger.Log(this, "Starting ConnectionKeeper routine", LogLevels.Info);
                MetricMessage("Dial()");

                KillOtherDialers();

                var modem = CheckModem();

                if (dialer.MaximumErrorsCountReached)
                {
                    ResetModem(modem);
                }

				PrepareConfig();

                MetricMessage("dialer.Start()");
                dialer.Start();
            }
            catch (Exception ex)
            {
                MetricMessage("EXCEPTION", ColoredStates.Red);
				logger.Log(this, "ConnectionKeeper routine interrupted with herror, restarting after 10 seconds...", LogLevels.Info);
                logger.Log(this, ex);
                Thread.Sleep(10000);
                Dial();
            }
        }

		private void PrepareConfig()
		{
            MetricMessage("PrepareConfig()");

			var ttyUsb = FindModem_ttyUSB();

			var dialConfigTemplatePath = Path.Combine (config.DataFolder, "wvdial.conf");
			var dialConfig = File.ReadAllText (dialConfigTemplatePath);
			dialConfig = string.Format (dialConfig, ttyUsb);

			var dialConfigPath = Path.Combine (config.DataFolder, "_wd.conf");
			File.WriteAllText (dialConfigPath, dialConfig);

			logger.Log (this, string.Concat ("Wvdial config file created for port: ", ttyUsb), LogLevels.Info);
		}

		private string FindModem_ttyUSB()
		{
			var ttyUsbFiles = Directory.GetFiles ("/dev", "ttyUSB*");

			if (!ttyUsbFiles.Any ())
				throw new Exception ("No any ttyUSB in the system");

			foreach (var file in ttyUsbFiles) 
			{
				string output = ProcessRunner.ExecuteTool ("Probe ttyUSB", (string o) => o, 20000, 
					"udevadm", 
					string.Concat("info --query=all -n ", file));

				if (null != output && output.Contains ("S: gsmmodem"))
					return file;
			}

			throw new Exception (string.Concat ("No appropriate gsm modem device found in ttyUSB list: ", string.Join (", ", ttyUsbFiles)));
		}

        private void KillOtherDialers()
        {
            MetricMessage("KillOtherDialers()");

			var psi = new ProcessStartInfo
			{
				FileName = "sudo",
				Arguments = "pkill " + dialerExe,
				UseShellExecute = false
			};

            using (var pr = new ProcessRunner(psi, false, false))
            {
                pr.Run();
                pr.WaitForExit(15000);
            }
        }

        private USBBusDevice GetModemDevice()
        {
            try
            {
                return NixHelpers.LsUsb.EnumerateDevices().Single(d =>
                    d.VID.Equals(modemVid, StringComparison.InvariantCultureIgnoreCase)
                    && (d.PID.Equals(modemPid_modemMode, StringComparison.InvariantCultureIgnoreCase) ||
                        d.PID.Equals(modemPid_storageMode, StringComparison.InvariantCultureIgnoreCase)));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Concat("Unable to locate modem device: ", ex.Message), ex);
            }
        }

        private USBBusDevice CheckModem()
        {
            MetricMessage("CheckModem()");

            USBBusDevice modem = null;

            int counter = 0;

            while (!(modem = GetModemDevice()).PID.Equals(modemPid_modemMode, StringComparison.InvariantCultureIgnoreCase))
            {
                if (++counter > 10)
                    throw new Exception("Unable to switch modem after 10 retries...");

                SwitchModem();

                Thread.Sleep(5000);
            }

            return modem;
        }

        private bool SwitchModem()
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
                ProcessRunner.TryExitEndDispose(pr);
            }
        }
    }
}
