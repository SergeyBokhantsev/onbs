using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                { "ttyUSBEnum", () => string.Join(Environment.NewLine, NixHelpers.DmesgFinder.EnumerateTTYUSBDevices(processRunnerFactory)) }
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
            const string ElmDeviceName = "COM_FTDI Device";
            return NixHelpers.DmesgFinder.FindTTYUSBPort(ElmDeviceName, processRunnerFactory);
        }
    }
}
