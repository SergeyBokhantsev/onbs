using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace HostController.Win
{
    public class MockConfigResolver : ConfigValuesResolver
    {
        private readonly Dictionary<string, string> values = new Dictionary<string, string>
        {
            { ConfigNames.Placeholder_Elm327Port, "COM0" },
            { ConfigNames.Placeholder_UIFullscreen, "False" },
            { ConfigNames.Placeholder_Vehicle, "TEST" }
        };

        protected override string GetValue(string placeholder)
        {
            return values[placeholder];
        }
    }
}
