using Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogLib
{
    public class GeneralLogger : FileLogWriter, ILogger
    {
        private readonly string logFolder;

        public event LogEventHandlerDelegate LogEvent;

        private readonly int startTime;

        public LogLevels Level
        {
            get;
            private set;
        }

        public DateTime LastWarningTime
        {
            get;
            private set;
        }

        public List<string> AllowedClassNames
        {
            get;
            private set;
        }

        public GeneralLogger(IConfig config)
            :base(20)
        {
            this.startTime = Environment.TickCount;

            var assemblyLocation = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            logFolder = Path.Combine(assemblyLocation, config.GetString(ConfigNames.LogFolder));

            Level = (LogLevels)Enum.Parse(typeof(LogLevels), config.GetString(ConfigNames.LogLevel));
            AllowedClassNames = new List<string>(config.GetString(ConfigNames.LoggedClasses).Split(',').Select(o => o.Trim()).Where(o => !string.IsNullOrWhiteSpace(o)));
        }

        public void Log(object caller, string message, LogLevels level)
        {
            if (level <= LogLevels.Warning)
                LastWarningTime = DateTime.Now;

            if (level <= this.Level)
            {
                string className = caller != null ? caller.GetType().ToString() : "NULL";

                if (caller == null || !AllowedClassNames.Any() || AllowedClassNames.Contains(className))
                {
					Add(string.Concat(GetTimestamp(), " | ", level, " | ", className, " | ", Thread.CurrentThread.ManagedThreadId, " | ", message, Environment.NewLine));
                }
            }
        }

        private string GetTimestamp()
        {
            int ticks = Environment.TickCount - startTime;
            int minutes = ticks / 60000;
            ticks -= minutes * 60000;
            int seconds = ticks / 1000;
            ticks -= seconds * 1000;
            int milliseconds = ticks;
            return string.Concat(minutes, ":", seconds, ".", milliseconds);
        }

        public void Log(object caller, Exception ex)
        {
            if (this.Level == LogLevels.Debug)
            {
                Log(caller, string.Concat(ex.Message, Environment.NewLine, ex.StackTrace), LogLevels.Error);
            }
            else
            {
                Log(caller, ex.Message, LogLevels.Error);
            }

            Flush();
        }

        public void Flush()
        {
            Save();
        }

        protected override string CreateFilePath(int index)
        {
            return System.IO.Path.Combine(logFolder, string.Format("GENERAL_{0}.txt", index));
        }
    }
}
