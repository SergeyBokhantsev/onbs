using System;
using System.Collections.Generic;
using System.Linq;
using Interfaces;

namespace HostController.Lin
{
    public class RPiConfigResolver : ConfigValuesResolver
    {
        private readonly IProcessRunnerFactory processRunnerFactory;
        private readonly Dictionary<string, object> cache = new Dictionary<string, object>();
        private readonly Dictionary<string, Func<string>> resolvers;

        public RPiConfigResolver(IProcessRunnerFactory processRunnerFactory)
        {
            this.processRunnerFactory = processRunnerFactory;

            resolvers = new Dictionary<string, Func<string>>
            {
                { ConfigNames.Placeholder_Elm327Port, GetElm327Port },
                { ConfigNames.Placeholder_UIFullscreen, () => "True" },
                { ConfigNames.Placeholder_Vehicle, () => "AH2392II" }
            };
        }

        protected override string GetValue(string placeholder)
        {
            if (resolvers.ContainsKey(placeholder))
                return resolvers[placeholder]();
            else
                return null;
        }

        private string GetElm327Port()
        {
            if (cache.ContainsKey(ConfigNames.Placeholder_Vehicle))
                return cache[ConfigNames.Placeholder_Vehicle] as string;

            const string vid = "0403";
            const string pid = "6001";

            var device = NixHelpers.DmesgFinder.FindUSBDevice(vid, pid, processRunnerFactory);

            if (device != null && device.AttachedTo.Any())
            {
                var ret = device.AttachedTo.First();
                cache.Add(ConfigNames.Placeholder_Vehicle, ret);
                return ret;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
