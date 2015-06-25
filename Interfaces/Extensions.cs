using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public static class Extensions
    {
        public static void InvertBoolSetting(this IConfig cfg, string name)
        {
            var value = cfg.GetBool(name);
            cfg.Set<bool>(name, !value);
        }
    }
}
