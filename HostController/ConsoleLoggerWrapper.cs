using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interfaces;
using System.Diagnostics;

namespace HostController
{
    public class ConsoleLoggerWrapper : ILogger
    {
        private readonly ILogger logger;

        public LogLevels Level
        {
            get
            {
                return logger.Level;
            }
        }

        internal ConsoleLoggerWrapper(ILogger logger)
        {
            this.logger = logger;
        }

        public void Log(object caller, string message, LogLevels level)
        {
            WriteToConsole(string.Concat(DateTime.Now, " | ", level, " | ", Thread.CurrentThread.ManagedThreadId, " | ", message));
            logger.Log(caller, message, level);
        }

        public void Log(object caller, Exception ex)
        {
            WriteToConsole(string.Concat(ex.Message, Environment.NewLine, ex.StackTrace));
            logger.Log(caller, ex);
        }

        [Conditional("DEBUG")]
        private void WriteToConsole(string message)
        {
            Console.WriteLine(message);
        }

        public void Flush()
        {
            logger.Flush();
        }
    }
}
