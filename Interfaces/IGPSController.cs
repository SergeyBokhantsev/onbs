﻿using System;
using Interfaces.GPS;

namespace Interfaces
{
    public interface IGPSController : IController
    {
        event Action<GPRMC> GPRMCReseived;
        event Action<string> NMEAReceived;

        GeoPoint Location {get;}

        int IdleMinutes { get; }
    }
}
