using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface ISessionConfig
    {
        bool IsSystemTimeValid { get; }
    }

    public interface IConfig : ISessionConfig
    {
        string GetString(string name);
        int GetInt(string name);
        double GetDouble(string name);
        bool GetBool(string name);
        void Set<T>(string name, T value);
        void Save();
    }
}
