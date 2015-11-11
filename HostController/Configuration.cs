using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using System.IO;
using System.Reflection;

namespace HostController
{
    public class Configuration : IConfig
    {        
        private readonly System.Configuration.Configuration cfg;
        private string dataFolder;

        #region SESSION CONFIG
        public bool IsSystemTimeValid
        {
            get;
            set;
        }

        public bool IsInternetConnected
        {
            get;
            set;
        }

        public string DataFolder
        {
            get
            {
                if (dataFolder == null)
                {
                    dataFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data");
                }

                return dataFolder;
            }
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
    }
}
