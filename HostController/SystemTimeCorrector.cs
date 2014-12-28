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
        private readonly IProcessRunnerFactory processRunnerFactory;
        private readonly ILogger logger;

        private readonly object locker = new object();
        private bool timeValid;

        public SystemTimeCorrector(IConfig config, IProcessRunnerFactory processRunnerFactory, ILogger logger)
        {
            this.processRunnerFactory = processRunnerFactory;
            this.logger = logger;

            minTimeDifference = config.GetDouble(Configuration.Names.SystemTimeMinDifference);
            setTimeCommand = config.GetString(Configuration.Names.SystemTimeSetCommand);
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

                    var pr = processRunnerFactory.Create(setTimeCommand, validTime.ToString("O"), true, false);
                    pr.Run();

					timeValid = true;
					return true;
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
