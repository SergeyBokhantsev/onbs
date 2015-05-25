using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPSD.Net.Messages
{
    internal class DeviceMsg : Message
    {
        public string path { get; set; }
        public double activated { get; set; }
        public int native { get; set; }
        public int bps { get; set; }
        public string parity { get; set; }
        public int stopbits { get; set; }
        public double cycle { get; set; }

        public DeviceMsg()
            :base("DEVICE")
        {
            path = "/dev/ttyUSB10";
            activated = 1269959537.20;
            native = 0;
            bps = 9600;
            parity = "N";
            stopbits = 1;
            cycle = 1;
        }
    }
}
