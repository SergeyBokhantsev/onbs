using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace ModemConnectionKeeper
{
    public class ConnectionMetricsProvider : MetricsProvider
    {
        public GenericMetric<string> PingMessage = new GenericMetric<string>("Ping message", String.Empty);
        public GenericMetric<string> DialerMessage = new GenericMetric<string>("Dialer message", String.Empty);
        public GenericMetric<int> DialerCriticalErrors = new GenericMetric<int>("Dialer critical errors", 0);
        public GenericMetric<string> KeeperMessage = new GenericMetric<string>("Keeper message", String.Empty);

        public ConnectionMetricsProvider(ILogger logger)
            : base(logger, "Internet connection")
        {
            Initialize(PingMessage, KeeperMessage, DialerMessage, DialerCriticalErrors);
        }
    }
}
