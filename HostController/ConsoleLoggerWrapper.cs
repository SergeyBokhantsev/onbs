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
            WriteToConsole(string.Concat(DateTime.Now, " | ", level, " | ", Thread.CurrentThread.ManagedThreadId, " | ", message), level);
            logger.Log(caller, message, level);
        }

        public void Log(object caller, Exception ex)
        {
            WriteToConsole(string.Concat(ex.Message, Environment.NewLine, ex.StackTrace), LogLevels.Error);
            logger.Log(caller, ex);
        }

        [Conditional("DEBUG")]
        private void WriteToConsole(string message, LogLevels level)
        {
            Console.WriteLine(message);

            //if (level < LogLevels.Info)
              //  Thread.Sleep(5000);
        }

        public void Flush()
        {
            logger.Flush();
        }
    }
}
