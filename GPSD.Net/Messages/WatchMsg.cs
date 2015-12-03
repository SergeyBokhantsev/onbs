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

        public static WatchMsg Parse(JsonObj json)
        {
            var ret = new WatchMsg
            {
                enable = JPath.GetFieldValue<bool>(json, "enable"),
                json = JPath.GetFieldValue<bool>(json, "json"),
                nmea = JPath.GetFieldValue<bool>(json, "nmea"),
                raw = JPath.GetFieldValue<int>(json, "raw"),
                timing = JPath.GetFieldValue<bool>(json, "timing"),
                scaled = JPath.GetFieldValue<bool>(json, "scaled"),
                pps = JPath.GetFieldValue<bool>(json, "pps")
            };

            return ret;
        }
    }
}
