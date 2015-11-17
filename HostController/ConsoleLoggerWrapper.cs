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
        private readonly ILogger[] loggers;

        public LogLevels Level
        {
            get
            {
                return logger.Level;
            }
        }

        internal ConsoleLoggerWrapper(ILogger[] loggers)
        {
            if (loggers == null)
                throw new ArgumentNullException("loggers");

            this.loggers = loggers;
        }

        public void Log(object caller, string message, LogLevels level)
        {
            WriteToConsole(string.Concat(DateTime.Now, " | ", level, " | ", Thread.CurrentThread.ManagedThreadId, " | ", message), level);

            foreach (var logger in loggers)
            {
                logger.Log(caller, message, level);
            }
        }

        public void Log(object caller, Exception ex)
        {
            WriteToConsole(string.Concat(ex.Message, Environment.NewLine, ex.StackTrace), LogLevels.Error);

            foreach (var logger in loggers)
            {
                logger.Log(caller, ex);
            }
        }

        [Conditional("DEBUG")]
        private void WriteToConsole(string message, LogLevels level)
        {
            Console.WriteLine(message);
        }

        public void Flush()
        {
            foreach (var logger in loggers)
            {
                logger.Flush();
            }
        }
    }
}
