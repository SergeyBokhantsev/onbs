using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interfaces;

namespace HostController
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

        public bool IsSystemTimeValid(DateTime validTime)
        {
            if (timeValid)
                return true;

            lock (locker)
            {
                if (timeValid)
                    return true;

                logger.Log(this, string.Format("Checking system time. System time is '{0}', proposed time is '{1}'", DateTime.Now.ToString(), validTime.ToString()), LogLevels.Info);

                if (Math.Abs((validTime - DateTime.Now).TotalMilliseconds) > minTimeDifference)
                {
                    logger.Log(this, "Updating system time...", LogLevels.Info);

                    var args = string.Format(setTimeArgs, validTime.ToString(setTimeSetFormat));
                    var pr = processRunnerFactory.Create(setTimeCommand, args, false);
                    pr.Run();

                    Thread.Sleep(500);
					return false;
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
