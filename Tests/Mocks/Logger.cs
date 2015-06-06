using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace Tests.Mocks
{
    public class Logger : ILogger
    {
        public LogLevels Level
        {
            get { return LogLevels.Debug; }
        }

        public void Log(object caller, string message, LogLevels level)
        {
        }

        public void Log(object caller, Exception ex)
        {
        }
    }
}
