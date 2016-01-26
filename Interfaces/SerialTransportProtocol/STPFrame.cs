using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Interfaces.SerialTransportProtocol
{
    public class STPFrame
    {
        private static int index;

		public enum Types
		{
			Undefined = 63, // ?
			GPS = 65, // A
			Button = 66, // B
			GSM = 67, // C
			ArduCommand = 68, // D
            MiniDisplay = 69,
            Relay = 70
		}

        public readonly Types Type;

        public byte[] Data { get; set; }

        public ushort Id { get; private set; }

		public bool Delivered { get; set; }

		public string String
		{
			get 
			{
				return string.Concat (Data.Select (b => 
				{
					if (b > 31 && b < 127)
						return ((char)b).ToString();
					else
						return ".";
				}));
			}
		}

        public override string ToString()
        {
            var bytes = Data != null ?
                string.Join(",", Data.Select(b => string.Concat("'", b, "'")))
                : "NULL";

            return string.Format("Type {0}, date lenght {1}, bytes: {2}", Type, Data.Length, bytes);
        }

        public STPFrame(byte[] data)
        {
            Data = data;
            Type = Types.Undefined;
            Id = 0;
        }

        public STPFrame(byte[] data, Types type)
        {
            Data = data;
            Type = type;
            var idInt = Interlocked.Increment(ref index);
            Id = (ushort)(idInt & 0xFFFF);
        }

        public STPFrame(byte[] data, Types type, ushort id)
        {
            Data = data;
            Type = type;
            Id = id;
        }

		public async Task<bool> WaitDeliveryAsync(int timeout)
		{
            if (Delivered)
                return await Task.FromResult(true);

			return await Task.Run (() => 
			{
				var startTime = DateTime.Now;
				while((DateTime.Now - startTime).TotalMilliseconds < timeout)
				{
					if (Delivered)
						return true;
					else
						Thread.Sleep(50);
				}

				return false;
			});
		}
    }
}
