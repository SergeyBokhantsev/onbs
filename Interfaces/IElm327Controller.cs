﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IElm327Controller : IController, IDisposable
    {
        string Error { get; }
        int? GetSpeed();
        int? GetRPM();
        double? GetFuelFlow();
        int? GetCoolantTemp();
        int? GetEngineLoad();
    }
}
