using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace HostController
{
    public class Configuration : IConfig
    {
        public static class Names
        {
            public static readonly string ArduinoPort = "ArduinoPort";
            public static readonly string ArduinoPortSpeed = "ArduinoPortSpeed";
            public static readonly string ArduinoPortParity = "ArduinoPortParity";
            public static readonly string ArduinoPortDataBits = "ArduinoPortDataBits";
            public static readonly string ArduinoPortStopBits = "ArduinoPortStopBits";

            public static readonly string LogLevel = "LogLevel";
            public static readonly string LoggedClasses = "LoggedClasses";
        }

        public string GetString(string name)
        {
            return ConfigurationManager.AppSettings[name];
        }

        public int GetInt(string name)
        {
            return int.Parse(ConfigurationManager.AppSettings[name]);
        }

        public double GetDouble(string name)
        {
            return double.Parse(ConfigurationManager.AppSettings[name]);
        }

        public bool GetBool(string name)
        {
            return bool.Parse(ConfigurationManager.AppSettings[name]);
        }
    }
}
