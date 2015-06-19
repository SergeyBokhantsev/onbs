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
            cfg.Save(ConfigurationSaveMode.Full);
        }

        public void Dispose()
        {
            Save();
        }
    }
}
