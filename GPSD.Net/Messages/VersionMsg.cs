namespace GPSD.Net.Messages
{
    internal class VersionMsg : Message
    {
        public string release { get; set; }
        public string rev { get; set; }
        public string proto_major { get; set; }
        public string proto_minor { get; set; }

        public VersionMsg()
            :base("VERSION")
        {
            release = "3.9";
            rev = "3.9";
            proto_major = "3";
            proto_minor = "8";
        }
    }
}
