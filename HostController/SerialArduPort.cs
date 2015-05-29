using Interfaces;
using System;
using System.IO.Ports;
namespace HostController
{
	public class SerialArduPort : IPort
	{
		private readonly SerialPort port;

		private readonly IDispatcher dispatcher;

		public SerialArduPort (IDispatcher dispatcher)
		{
			if (dispatcher == null)
				throw new ArgumentNullException ("dispatcher");

			this.dispatcher = dispatcher;

			port = new SerialPort ("/dev/ttyAMA0", 115200, Parity.None, 8, StopBits.One);
			port.Open ();

			new System.Threading.Thread (() => Monitor ()).Start ();
		}

		private void Monitor()
		{
			while (true) 
			{
				if (port.BytesToRead > 0) {
					OnDataReceived ();
				} else
					System.Threading.Thread.Sleep (50);
			}
		}

		void OnDataReceived ()
		{
			var handler = DataReceived;
			if (handler != null)
				handler (this, null);
		}

		#region IPort implementation

		public event SerialDataReceivedEventHandler DataReceived;

		public int Read (byte[] buffer, int offset, int count)
		{
			return port.Read (buffer, offset, count);
		}

		#endregion
	}
}

