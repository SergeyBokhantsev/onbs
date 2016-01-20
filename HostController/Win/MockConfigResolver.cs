using System.Collections.Generic;
using Interfaces;

namespace HostController.Win
{
    public class MockConfigResolver : ConfigValuesResolver
    {
        private readonly Dictionary<string, string> values = new Dictionary<string, string>
        {
            { ConfigNames.Placeholder_Elm327Port, "COM0" },
            { ConfigNames.Placeholder_UIFullscreen, "False" },
            { ConfigNames.Placeholder_Vehicle, "TEST" },
            { ConfigNames.Placeholder_ArduinoConfirmationTimeout, "2500" }
        };

        protected override string GetValue(string placeholder)
        {
            return values[placeholder];
        }
    }
}
