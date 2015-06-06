using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interfaces;

namespace Tests.Mocks
{
    public class GPSController : IGPSController
    {
        public event Action<Interfaces.GPS.GPRMC> GPRMCReseived;

        public event Action<string> NMEAReceived;

        public GPSController()
        {
            new Thread(() => DoFake()).Start();
        }

        private void DoFake()
        {
            while (true)
            {
                var handler = NMEAReceived;
                if (handler != null)
                    handler("$GPRMC");

                Thread.Sleep(1000);
            }
        }

        public event MetricsUpdatedEventHandler MetricsUpdated;
    }
}
