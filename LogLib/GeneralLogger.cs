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

        public LogLevels Level
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
            var assemblyLocation = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            logFolder = Path.Combine(assemblyLocation, config.GetString(ConfigNames.LogFolder));

            Level = (LogLevels)Enum.Parse(typeof(LogLevels), config.GetString(ConfigNames.LogLevel));
            AllowedClassNames = new List<string>(config.GetString(ConfigNames.LoggedClasses).Split(',').Select(o => o.Trim()).Where(o => !string.IsNullOrWhiteSpace(o)));
        }

        public void Log(object caller, string message, LogLevels level)
        {
            if (level <= this.Level)
            {
                string className = caller != null ? caller.GetType().ToString() : "NULL";

                if (caller == null || !AllowedClassNames.Any() || AllowedClassNames.Contains(className))
                {
					Add(string.Concat(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss.fff"), " | ", level, " | ", className, " | ", Thread.CurrentThread.ManagedThreadId, " | ", message, Environment.NewLine));
                }
            }
        }

        public void Log(object caller, Exception ex)
        {
            Log(caller, string.Concat(ex.Message, Environment.NewLine, ex.StackTrace), LogLevels.Error);
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
