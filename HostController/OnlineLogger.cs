using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using TravelsClient;

namespace HostController
{
    public class OnlineLogger : ILogger
    {
        private readonly StringBuilder buffer = new StringBuilder();
        private readonly GeneralLoggerClient client;

        public LogLevels Level
        {
            get;
            private set;
        }

        public OnlineLogger(IConfig config)
        {
            Level = (LogLevels)Enum.Parse(typeof(LogLevels), config.GetString(ConfigNames.LogLevel));
            client = new GeneralLoggerClient
        }

        public void Log(object caller, string message, LogLevels level)
        {
            if (level <= this.Level)
            {
                string className = caller != null ? caller.GetType().ToString() : "NULL";

                Add(string.Concat(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss.fff"), " | ", level, " | ", className, " | ", Thread.CurrentThread.ManagedThreadId, " | ", message, Environment.NewLine));
            }
        }

        private void Add(string p)
        {
            lock (buffer)
            {
                buffer.Append(p);
            }
        }

        public void Log(object caller, Exception ex)
        {
            Log(caller, string.Concat(ex.Message, Environment.NewLine, ex.StackTrace), LogLevels.Error);
            Flush();
        }

        public void Flush()
        {
            var body = null;

            lock (buffer)
            {
                if (buffer.Length == 0)
                    return;

                body = buffer.ToString();
                buffer.Clear();
            }

            
        }
    }
}
