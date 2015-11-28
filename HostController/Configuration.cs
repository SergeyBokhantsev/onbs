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
    public abstract class ConfigValuesResolver
    {
        private const string PlaceholderBegin = "{_";
        private const string PlaceholderEnd = "_}";

        public string Resolve(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var begInd = input.IndexOf(PlaceholderBegin);

            if (begInd != -1)
            {
                var endInd = input.IndexOf(PlaceholderEnd);

                if (endInd != -1 && endInd > begInd)
                {
                    var placeholder = input.Substring(begInd + PlaceholderBegin.Length, endInd - (begInd + PlaceholderBegin.Length));
                    var placeholderValue = GetValue(placeholder);

                    if (placeholderValue == null)
                        throw new Exception(string.Format("Unable to resolve config value for '{0}' placeholder", placeholder));

                    var resolvedValue = string.Concat(input.Substring(0, begInd), placeholderValue, input.Substring(endInd + PlaceholderEnd.Length));
                    return resolvedValue;
                }
            }

            return input;
        }

        /// <summary>
        /// Derived implementation should return NULL only if placeholder is unknown. Othervise should return string value (string.Empty in case if resolved value is null)
        /// </summary>
        protected abstract string GetValue(string placeholder);
    }

    public class Configuration : IConfig
    {
        private readonly ConfigValuesResolver valuesResolver;
        private readonly System.Configuration.Configuration cfg;
        private string dataFolder;

        #region SESSION CONFIG

        public Environments Environment
        {
            get;
            set;
        }

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

        public Configuration(ConfigValuesResolver valuesResolver)
        {
            if (valuesResolver == null)
                throw new ArgumentNullException("valuesResolver");

            this.valuesResolver = valuesResolver;

            cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

        public string GetString(string name)
        {
            var v = cfg.AppSettings.Settings[name];

            if (v == null)
                throw new Exception(string.Format("Config setting is not exist: {0}", name));

            return valuesResolver.Resolve(v.Value);
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
