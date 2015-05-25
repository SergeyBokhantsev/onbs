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

        public void Log(string message, LogLevels level)
        {
        }

        public void Log(Exception ex)
        {
        }
    }
}
