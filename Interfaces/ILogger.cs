using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public enum LogLevels
    {
        Fatal = 0,
        Error = 1,
        Warning = 2,
        Info = 3,
        Debug = 4
    }

    public interface ILogger
    {
        void Log(object caller, string message, LogLevels level);
        void Log(object caller, Exception ex);
        void Flush();
    }

    public static class LogHelper
    {
        [Conditional("DEBUG")]
        public static void LogIfDebug(this ILogger logger, object caller, string message, LogLevels level = LogLevels.Debug)
        {
            logger.Log(caller, message, level);
        }
    }
}
