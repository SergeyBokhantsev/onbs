using System.Text;

namespace Interfaces.SerialTransportProtocol
{
    public class STPFrame
    {
		public enum Types
		{
			Undefined = 63, // ?
			GPS = 65, // A
			Button = 66, // B
			GSM = 67, // C
			ArduCommand = 68, // D
            MiniDisplay = 69
		}

        public readonly Types Type;

        public byte[] Data { get; set; }

		public string String
		{
			get 
			{
				return Encoding.Default.GetString (Data);
			}
		}

		public override string ToString ()
		{
			return string.Format ("[{0}, {1} bytes]", Type, Data.Length);
		}

        public STPFrame(byte[] data, Types type)
        {
            Data = data;
            Type = type;
        }
    }
}
