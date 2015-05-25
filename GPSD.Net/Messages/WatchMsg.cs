using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Json;

namespace GPSD.Net.Messages
{
    internal class WatchMsg : Message
    {
        public bool enable { get; set; }
        public bool json { get; set; }
        public bool nmea { get; set; }
        public int raw { get; set; }
        public bool scaled { get; set; }
        public bool timing { get; set; }
        public bool pps { get; set; }

        public WatchMsg()
            :base("WATCH")
        {
            enable = true;
            json = true;
            nmea = false;
            raw = 0;
            scaled = true;
            timing = false;
            pps = false;
        }

        public static WatchMsg Parse(Json.JsonObj json)
        {
            var ret = new WatchMsg();

            ret.enable = JPath.GetFieldValue<bool>(json, "enable");
            ret.json = JPath.GetFieldValue<bool>(json, "json");
            ret.nmea = JPath.GetFieldValue<bool>(json, "nmea");
            ret.raw = JPath.GetFieldValue<int>(json, "raw");
            ret.timing = JPath.GetFieldValue<bool>(json, "timing");
            ret.scaled = JPath.GetFieldValue<bool>(json, "scaled");
            ret.pps = JPath.GetFieldValue<bool>(json, "pps");

            return ret;
        }
    }
}
