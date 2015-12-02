namespace GPSD.Net.Messages
{
    internal class DevicesMsg : Message
    {
        public object[] devices { get; set; }

        public DevicesMsg()
            :base("DEVICES")
        {
            devices = new object[] { new DeviceMsg() };
        }
    }
}
