using System;
using System.Threading;
using Interfaces;
using System.Diagnostics;

namespace HostController
{
    public class ConsoleLoggerWrapper : ILogger
    {
        private readonly ILogger[] loggers;
        internal ConsoleLoggerWrapper(ILogger[] loggers)
        {
            if (loggers == null)
                throw new ArgumentNullException("loggers");

            this.loggers = loggers;
        }

        public void Log(object caller, string message, LogLevels level)
        {
            WriteToConsole(string.Concat(DateTime.Now, " | ", level, " | ", Thread.CurrentThread.ManagedThreadId, " | ", message));

            foreach (var logger in loggers)
            {
                logger.Log(caller, message, level);
            }
        }

        public void Log(object caller, Exception ex)
        {
            WriteToConsole(string.Concat(ex.Message, Environment.NewLine, ex.StackTrace));

            foreach (var logger in loggers)
            {
                logger.Log(caller, ex);
            }
        }

        [Conditional("DEBUG")]
        private void WriteToConsole(string message)
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
