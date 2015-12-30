using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.Relays
{
    public static class RelayCommands
    {
        public const byte TURN = 0;
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
        void Enable(Relay relay);
        void Disable(Relay relay);
    }
}
