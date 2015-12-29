using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Interfaces.SerialTransportProtocol;

namespace ArduinoBoardEmulator
{
    internal class RelayDescriptor
    {
        public Interfaces.Relays.Relay Name { get; set; }
        public bool Enabled { get; set; }
        public int RemainingDelay { get; set; }
        public bool ScheduledAction {get; set; }
    }

    internal class Relay
    {
        public event Action<RelayDescriptor> RelayUpdated;

        private readonly RelayDescriptor[] relays = new RelayDescriptor[]
        {
            new RelayDescriptor { Name = Interfaces.Relays.Relay.Master },
            new RelayDescriptor { Name = Interfaces.Relays.Relay.OBD },
            new RelayDescriptor { Name = Interfaces.Relays.Relay.Relay3 },
            new RelayDescriptor { Name = Interfaces.Relays.Relay.Relay4 }
        };

        public Relay()
        {
            Timer t = new Timer(1000);
            t.Elapsed += Update;
            t.Start();
        }

        public void Update(object sender, ElapsedEventArgs e)
        {
            foreach(var relay in relays)
            {
                if (relay.RemainingDelay > 0)
                {
                    relay.RemainingDelay -= 1;
                    
                    if (relay.RemainingDelay == 0)
                    {
                        relay.Enabled = relay.ScheduledAction;
                    }
                }

                RelayUpdated(relay);
            }
        }

        internal void ProcessFrame(STPFrame frame)
        {
            switch (frame.Data[0])
            {
                case Interfaces.Relays.RelayCommands.SCHEDULE_COMMAND:
                    {
                        var relName = (Interfaces.Relays.Relay)frame.Data[1];
                        var act = (Interfaces.Relays.RelayActions)frame.Data[2] > 0;
                        var delaySec = frame.Data[3];

                        var relay = relays.First(r => r.Name == relName);

                        if (delaySec > 0)
                        {
                            relay.RemainingDelay = delaySec;
                            relay.ScheduledAction = act;
                        }
                        else
                        {
                            relay.Enabled = act;
                            relay.RemainingDelay = 0;
                        }
                    }
                    break;

                case Interfaces.Relays.RelayCommands.UNSCHEDULE_COMMAND:
                    {
                        var relName = (Interfaces.Relays.Relay)frame.Data[1];
                        var relay = relays.First(r => r.Name == relName);
                        relay.RemainingDelay = 0;
                    }
                    break;
            }

            Update(null, null);
        }
    }
}
