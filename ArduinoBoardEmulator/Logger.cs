using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoBoardEmulator
{
    public class Logger : ILogger
    {
        public LogLevels Level
        {
            get { return LogLevels.Info; }
        }

        public void Log(object caller, string message, LogLevels level)
        {
            
        }

        public void Log(object caller, Exception ex)
        {
            
        }

        public void Flush()
        {
            
        }
    }
}
