using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPSD.Net.Messages
{
    internal class TPVMsg : Message
    {
        public string time { get; set; }
        public double ept { get; set; }
        public double lat { get; set; }
        public double lon { get; set; }
        public double alt { get; set; }
        public double epx { get; set; }
        public double epy { get; set; }
        public double epv { get; set; }
        public double track { get; set; }
        public double speed { get; set; }
        public double climb { get; set; }
        public double eps { get; set; }
        public int mode { get; set; }

        public TPVMsg()
            :base("TPVMsg")
        {
        }
    }
}
