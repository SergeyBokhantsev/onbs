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
        private readonly object locker = new object();

        public LogLevels Level
        {
            get { return LogLevels.Debug; }
        }

        public List<string> AllowedClassNames
        {
            get;
            private set;
        }

        internal ConsoleLoggerWrapper()
        {
            AllowedClassNames = new List<string>();
        }

        public void Log(object caller, string message, LogLevels level)
        {
            if (level <= this.Level)
            {
                var className = GetClassName(caller);

                if (!AllowedClassNames.Any() || AllowedClassNames.Contains(className))
                {
                    lock (locker)
                    {
                        Console.WriteLine(string.Concat(DateTime.Now, " | ", level, " | ", className, " | ", Thread.CurrentThread.ManagedThreadId, " | ", message));
                    }
                }
            }
        }

        public void Log(object caller, Exception ex)
        {
            Log(caller, string.Concat(ex.Message, Environment.NewLine, ex.StackTrace), LogLevels.Error);
        }

        private string GetClassName(object caller)
        {
            if (caller == null)
                return "Unknown";

            var callerType = caller.GetType();
            var classNameAttr = callerType.GetCustomAttributes(typeof(LogClassAttribute), true).FirstOrDefault() as LogClassAttribute;
            return classNameAttr != null ? classNameAttr.ClassName : callerType.Name;
        }
    }
}
