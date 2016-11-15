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
        }

        void DialerProcessExited()
        {
            logger.Log(this, "Dialer exited, restarting after 10 seconds...", LogLevels.Info);

            Thread.Sleep(10000);

            Dial();
        }

        private void ResetModem(USBBusDevice modem)
        {
            var output = ProcessRunner.ExecuteTool("Reset modem", (string o) => o,
                          15000, 
                          "sudo",
                          Path.Combine(config.DataFolder, string.Format("usbreset /dev/bus/usb/{0}/{1}", modem.Bus, modem.Device)));

            if (string.IsNullOrEmpty(output) || !output.Contains("Reset successful"))
                throw new Exception(string.Concat("Unable to reset modem: ", output));
        }

        private void Dial()
        {
            try
            {
                KillOtherDialers();

                var modem = CheckModem();

                if (dialer.MaximumErrorsCountReached)
                {
                    ResetModem(modem);
                }

                dialer.Start();
            }
            catch (Exception ex)
            {
                logger.Log(this, ex);
                Thread.Sleep(10000);
                Dial();
            }
        }

        private void KillOtherDialers()
        {
            var counter = 0;

            while ((NixHelpers.ProcessFinder.FindProcess(dialerExe)) != -1)
            {
                if (++counter > 10)
                    throw new Exception("Cannot kill other dealers after 10 retries");

                var psi = new ProcessStartInfo
                {
                    FileName = "sudo",
                    Arguments = "pkill " + dialerExe,
                    UseShellExecute = false
                };

                var pr = new ProcessRunner(psi, false, false);
                pr.Run();

                Thread.Sleep(5000);
            }
        }

        private USBBusDevice GetModemDevice()
        {
            try
            {
                return NixHelpers.LsUsb.EnumerateDevices().SingleOrDefault(d =>
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
                if (null != pr && !pr.HasExited)
                    pr.Exit();
            }
        }
    }
}
