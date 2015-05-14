using System;
using System.Collections.Generic;
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
        LogLevels Level { get; }
        void Log(string message, LogLevels level);
    }
}
