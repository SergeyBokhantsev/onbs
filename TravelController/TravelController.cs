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
        //private enum States { NotStarted, FindingOpenedTravel, CreatingTravel, Ready }

        private readonly IHostController hc;
        private readonly AsyncClient client;
        private readonly List<TravelPoint> points = new List<TravelPoint>();

        private Travel travel;
        private bool ready;

        //private LockingProperty<States> state = new LockingProperty<States>(States.NotStarted);

        public TravelController(IHostController hc)
        {
            if (hc == null)
                throw new ArgumentNullException("hc");

            this.hc = hc;
            
            if ((client = CreateClient(hc.Config, hc.Logger)) != null)
            {
                hc.GetController<IGPSController>().GPRMCReseived += GPRMCReseived;
                hc.Dispatcher.CreateTimer(30, ExportPoints).Enabled = true;
            }
        }

        private void FindOrCreateCurrentTravel()
        {
            if (client.TryFindActiveTravelAsync(travel =>
                {
                    this.travel = travel;

                    if (travel == null)
                    {
                        
                    }
                    else
                    {

                    }
                }))
            {
                
            }
        }

        private void ExportPoints(object sender, EventArgs e)
        {
            if (!ready)
            {
               // FindOrCreateCurrentTravel();
            }
        }

        private void GPRMCReseived(GPRMC obj)
        {

        }

        private AsyncClient CreateClient(IConfig config, ILogger logger)
        {
            try
            {
                if (config == null)
                    throw new ArgumentNullException("config");

                var serverUrl = config.GetString(ConfigNames.TravelServiceUrl);
                var vehicleId = config.GetString(ConfigNames.TravelServiceVehicle);
                var key = config.GetString(ConfigNames.TravelServiceKey);

                return new AsyncClient(new Uri(serverUrl), key, vehicleId, logger);
            }
            catch (Exception ex)
            {
                logger.Log(this, "Unable to create TravelClient", LogLevels.Error);
                logger.Log(this, ex);
                return null;
            }
        }
    }
}
