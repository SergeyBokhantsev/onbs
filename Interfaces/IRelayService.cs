using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.Relays
{
    public static class RelayCommands
    {
        public const byte UNSCHEDULE_COMMAND = 0;
        public const byte SCHEDULE_COMMAND = 1;
    }

    public enum Relay 
    { 
        Master = 0, 
        OBD = 1, 
        Relay3 = 2, 
        Relay4 = 3
    };

    public enum RelayActions
    {
        Enable = 1,
        Disable = 0
    };

    public interface IRelayService
    {
        void Schedule(Relay relay, RelayActions action, byte delaySec);
        void Unschedule(Relay relay);
    }
}
