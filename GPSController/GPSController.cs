﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interfaces;

namespace GPSController
{
    public class GPSController : IGPSController
    {
        public event Action<Interfaces.GPS.GPRMC> GPRMCReseived;
        public event Action<string> NMEAReceived;

        private readonly IDispatcher dispatcher;

        public GPSController(IDispatcher dispatcher)
        {
            if (dispatcher == null)
                throw new ArgumentNullException("dispatcher");

            this.dispatcher = dispatcher;

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
    }
}
