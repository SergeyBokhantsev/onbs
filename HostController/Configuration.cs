using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace HostController
{
    public class Configuration : IConfig, IDisposable
    {
        public static class Names
        {
            public static readonly string UIHostAssemblyName = "UIHostAssemblyName";
            public static readonly string UIHostClass = "UIHostClass";

            public static readonly string ArduinoPortFake = "ArduinoPortFake";
            public static readonly string ArduinoPort = "ArduinoPort";
            public static readonly string ArduinoPortEnabled = "ArduinoPortEnabled";
            public static readonly string ArduinoPortSpeed = "ArduinoPortSpeed";
            public static readonly string ArduinoPortParity = "ArduinoPortParity";
            public static readonly string ArduinoPortDataBits = "ArduinoPortDataBits";
            public static readonly string ArduinoPortStopBits = "ArduinoPortStopBits";

            public static readonly string LogLevel = "LogLevel";
            public static readonly string LoggedClasses = "LoggedClasses";

            public static readonly string SystemTimeMinDifference = "SystemTimeMinDifference";
            public static readonly string SystemTimeSetCommand = "SystemTimeSetCommand";
            public static readonly string SystemTimeValidByDefault = "SystemTimeValidByDefault";
        }

        private readonly System.Configuration.Configuration cfg;

        #region SESSION CONFIG
        public bool IsSystemTimeValid
        {
            get;
            set;
        }

        #endregion

        public Configuration()
        {
            cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

        public string GetString(string name)
        {
            return cfg.AppSettings.Settings[name].Value;
        }

        public int GetInt(string name)
        {
            return int.Parse(GetString(name));
        }

        public double GetDouble(string name)
        {
            return double.Parse(GetString(name));
        }

        public bool GetBool(string name)
        {
            return bool.Parse(GetString(name));
        }

        public void Set<T>(string name, T value)
        {
            cfg.AppSettings.Settings[name].Value = value.ToString();
        }

        public void Save()
        {
            cfg.Save(ConfigurationSaveMode.Modified);
        }

        public void Dispose()
        {
            Save();
        }
    }
}
