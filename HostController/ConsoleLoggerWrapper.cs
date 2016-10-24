using System;
using System.Threading;
using Interfaces;
using System.Diagnostics;
using System.Linq;

namespace HostController
{
    public class ConsoleLoggerWrapper : ILogger
    {
        public event LogEventHandlerDelegate LogEvent;

        private readonly ILogger[] loggers;

        public DateTime LastWarningTime
        {
            get { return loggers.Max(l => l.LastWarningTime); }
        }

        internal ConsoleLoggerWrapper(ILogger[] loggers)
        {
            if (loggers == null)
                throw new ArgumentNullException("loggers");

            this.loggers = loggers;
        }

        public void Log(object caller, string message, LogLevels level)
        {
            if (caller != null && caller is UIModels.ModelBase)
                WriteToConsole(string.Concat(DateTime.Now, " | ", level, " | ", Thread.CurrentThread.ManagedThreadId, " | ", message));

            foreach (var logger in loggers)
            {
                logger.Log(caller, message, level);
            }

            OnLogEvent(caller, message, level);
        }

        public void Log(object caller, Exception ex)
        {
            WriteToConsole(string.Concat(ex.Message, Environment.NewLine, ex.StackTrace));

            foreach (var logger in loggers)
            {
                logger.Log(caller, ex);
            }

            OnLogEvent(caller, ex.Message, LogLevels.Error);
        }

        private void OnLogEvent(object caller, string message, LogLevels level)
        {
            var handler = LogEvent;

            if (null != handler)
                handler(caller, message, level);
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
