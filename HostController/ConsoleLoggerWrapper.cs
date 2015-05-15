using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        }

        public void Log(string message, LogLevels level)
        {
            if (level <= this.Level)
            {
                Console.WriteLine(string.Concat(DateTime.Now, " | ", level, " | ", Thread.CurrentThread.ManagedThreadId, " | ", message));
            }
        }

        public void Log(Exception ex)
        {
            Log(string.Concat(ex.Message, Environment.NewLine, ex.StackTrace), LogLevels.Error);
        }
    }
}
