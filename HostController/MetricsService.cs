using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace HostController
{
    public class MetricsService : IMetricsService
    {
        private readonly ILogger logger;

        private readonly List<IMetricsProvider> providers = new List<IMetricsProvider>();

        public IEnumerable<IMetricsProvider> Providers
        {
            get { return providers; }
        }

        public MetricsService(ILogger logger)
        {
            if (null == logger)
                throw new ArgumentNullException("logger");

            this.logger = logger;
        }

        public void RegisterProvider(IMetricsProvider provider)
        {
            if (!providers.Contains(provider))
            {
                providers.Add(provider);
                logger.Log(this, string.Concat("Metrics provider registered: ", provider.Name), LogLevels.Info);
            }
        }

        public void UnregisterProvider(IMetricsProvider provider)
        {
            providers.Remove(provider);
            logger.Log(this, string.Concat("Metrics provider unregistered: ", provider.Name), LogLevels.Info);
        }
    }
}
