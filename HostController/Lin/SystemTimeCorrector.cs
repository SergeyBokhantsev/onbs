﻿using System;
using System.Threading.Tasks;
using Interfaces;

namespace HostController.Lin
{
    internal class SystemTimeCorrector
    {
        private readonly double minTimeDifference;
        private readonly string setTimeCommand;
        private readonly string setTimeArgs;
        private readonly string setTimeSetFormat;
        private readonly IProcessRunnerFactory processRunnerFactory;
        private readonly ILogger logger;

        private static readonly object locker = new object();

        public SystemTimeCorrector(IConfig config, IProcessRunnerFactory processRunnerFactory, ILogger logger)
        {
            if (processRunnerFactory == null || logger == null)
                throw new ArgumentNullException("processRunnerFactory OR logger");

            this.processRunnerFactory = processRunnerFactory;
            this.logger = logger;

            minTimeDifference = config.GetDouble(ConfigNames.SystemTimeMinDifference);
            setTimeCommand = config.GetString(ConfigNames.SystemTimeSetCommand);
            setTimeArgs = config.GetString(ConfigNames.SystemTimeSetArgs);
            setTimeSetFormat = config.GetString(ConfigNames.SystemTimeSetFormat);

            if (string.IsNullOrEmpty(setTimeCommand) || string.IsNullOrEmpty(setTimeArgs) || string.IsNullOrEmpty(setTimeSetFormat))
                throw new ArgumentNullException("setTimeCommand OR setTimeArgs OR setTimeSetFormat");
        }

		private bool GetTimeValidity(DateTime time)
		{
			var msDelta = (time.ToLocalTime() - DateTime.Now).TotalMilliseconds;
			return Math.Abs(msDelta) < minTimeDifference;
		}

        public bool IsSystemTimeValid(DateTime proposedTime)
        {
			if (proposedTime.Year < 2016 || proposedTime.Year > 2017)
            {
                logger.Log(this, string.Format("Proposed time '{0}' is invalid, ignoring.", proposedTime), LogLevels.Info);
                return false;
            }

            lock (locker)
            {
                logger.Log(this, string.Format("Checking system time. System time is '{0}', proposed time is '{1}'", DateTime.Now.ToString(), proposedTime.ToString()), LogLevels.Info);

                if (!GetTimeValidity(proposedTime))
                {
					logger.Log(this, string.Format("Updating system time..."), LogLevels.Info);

                    try
                    {
                        var processConfig = new ProcessConfig
                        {
                            ExePath = setTimeCommand,
							Args = string.Format(setTimeArgs, proposedTime.ToLocalTime().ToString(setTimeSetFormat)),
                            RedirectStandardOutput = false,
                            RedirectStandardInput = false
                        };

                        var pr = processRunnerFactory.Create(processConfig);
                        pr.Run();
                        pr.WaitForExit(5000);

                        return true;
                    }
                    catch (Exception ex)
                    {
                        logger.Log(this, ex);
                        return false;
                    }
                }
                else
                {
                    logger.Log(this, "System time is valid.", LogLevels.Info);
                    return true;
                }
            }
        }
    }
}
