using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace Tests.Mocks
{
    public class Config : IConfig
    {
        public readonly Dictionary<string, object> Properties = new Dictionary<string, object>();

        public string GetString(string name)
        {
            return (string)Properties[name];
        }

        public int GetInt(string name)
        {
            return (int)Properties[name];
        }

        public double GetDouble(string name)
        {
            return (double)Properties[name];
        }

        public bool GetBool(string name)
        {
            return (bool)Properties[name];
        }

        public void Set<T>(string name, T value)
        {
            Properties[name] = value;

            var handler = Changed;
            if (handler != null)
                handler(name);
        }

        public void Save()
        {
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
            get { throw new NotImplementedException(); }
        }

        public Environments Environment
        {
            get { return Environments.Win; }
        }


        public bool IsGPSLock
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsMessagePending
        {
            get { throw new NotImplementedException(); }
        }

        public event Action<string> Changed;


        public bool IsMessageShown
        {
            get { throw new NotImplementedException(); }
        }
    }
}
