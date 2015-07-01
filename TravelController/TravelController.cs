using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.GPS;
using TravelsClient;

namespace TravelController
{
    public class TravelController : IController
    {
        private readonly IHostController hc;
        private readonly Client client;

        public TravelController(IHostController hc)
        {
            if (hc == null)
                throw new ArgumentNullException("hc");

            this.hc = hc;
            this.client = CreateClient(hc.Config, hc.Logger);

            hc.GetController<IGPSController>().GPRMCReseived += GPRMCReseived;
        }

        void GPRMCReseived(GPRMC obj)
        {
            
        }

        private Client CreateClient(IConfig config, ILogger logger)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            var serverUrl = config.GetString(ConfigNames.TravelServiceUrl);
            var vehicleId = config.GetString(ConfigNames.TravelServiceVehicle);
            var key = config.GetString(ConfigNames.TravelServiceKey);

            return new Client(serverUrl, key, vehicleId, logger);
        }
    }
}
