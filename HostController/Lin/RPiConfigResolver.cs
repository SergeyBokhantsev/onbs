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
                { ConfigNames.Placeholder_Vehicle, () => "AH2392II" },
                { ConfigNames.Placeholder_ArduinoConfirmationTimeout, () => "200" }
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
            if (cache.ContainsKey(ConfigNames.Placeholder_Elm327Port))
                return cache[ConfigNames.Placeholder_Elm327Port] as string;

            const string vid = "0403";
            const string pid = "6001";

            try
            {
                var device = NixHelpers.DmesgFinder.FindUSBDevice(vid, pid, processRunnerFactory);

                if (device != null && device.AttachedTo.Any())
                {
                    var ret = device.AttachedTo.First();
                    cache.Add(ConfigNames.Placeholder_Elm327Port, ret);
                    //logger.Log(this, string.Format("ELm327 port resolved as {0}", ret), LogLevels.Info);
                    return ret;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                //logger.Log(this, ex);
                return string.Empty;
            }
        }
    }
}
