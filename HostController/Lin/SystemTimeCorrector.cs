using System;
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
        private static bool timeValid;

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
			return Math.Abs((time - DateTime.Now).TotalMilliseconds) < minTimeDifference;
		}

        public bool IsSystemTimeValid(DateTime validTime)
        {
            if (timeValid)
                return true;

            lock (locker)
            {
                if (timeValid)
                    return true;

                logger.Log(this, string.Format("Checking system time. System time is '{0}', proposed time is '{1}'", DateTime.Now.ToString(), validTime.ToString()), LogLevels.Info);

                if (!GetTimeValidity(validTime)) 
                {
                    logger.Log(this, "Updating system time...", LogLevels.Info);

                    try
                    {
                        var processConfig = new ProcessConfig
                        {
                            ExePath = setTimeCommand,
                            Args = string.Format(setTimeArgs, validTime.ToString(setTimeSetFormat)),
                        };

                        var pr = processRunnerFactory.Create(processConfig);
                        pr.Run();
                        pr.WaitForExit(5000);
                        if (GetTimeValidity(validTime))
                            return IsSystemTimeValid(validTime);
                        else
                            return false;
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
                    timeValid = true;
                    return true;
                }
            }
        }
    }
}
