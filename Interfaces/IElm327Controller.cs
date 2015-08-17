using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IElm327Controller : IController
    {
        event Action<IElm327Response> ResponceReseived;
        void Request(Elm327FunctionTypes type);
    }

    public interface IElm327Response
    {
        Elm327FunctionTypes Type { get; }
    }

    public enum Elm327FunctionTypes : uint
    { 
        Error = 0xFFFFFF,
        RawString = 0xFFFFFE,
        SupportedFunctions = 0x0100,
        MonitorStatus = 0x0101,
        FuelSystemStatus = 0x0103,
        EngineRPM = 0x010C,
        Speed = 0x010D,
    };
}
