namespace GPSD.Net.Messages
{
    internal abstract class Message
    {
        public string @class { get; private set; }

        protected Message(string className)
        {
            @class = className;
        }
    }
}
