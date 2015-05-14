using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace HostController
{
    public class ConsoleLoggerWrapper : ILogger
    {
        public LogLevels Level
        {
            get { return LogLevels.Debug; }
        }

        internal ConsoleLoggerWrapper()
        {
            Log("--- Logging initiated ---", LogLevels.Info);
        }

        public void Log(string message, LogLevels level)
        {
            if (level <= this.Level)
            {
                Console.WriteLine(string.Concat(DateTime.Now, " | ", level, " | ", message));
            }
        }
    }
}
