using Interfaces;
using System;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
namespace HostController
{
    [LogClass("Serial Port")]
	public class SerialArduPort : IPort
	{
        public event SerialDataReceivedEventHandler DataReceived;

		private readonly SerialPort port;

        private readonly ILogger logger;

        public SerialArduPort(ILogger logger, string portPath)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");

            this.logger = logger;

            port = new SerialPort(portPath, 115200, Parity.None, 8, StopBits.One);
            
            new System.Threading.Thread(() => Monitor()).Start();
        }

        private void Monitor()
        {
            Thread.CurrentThread.Name = "Serial";

            while (true)
            {
            int delay;

            try
            {
                if (!port.IsOpen)
                    port.Open();

                while (true)
                {
                    lock (port)
                    {
                        if (port.BytesToRead > 0)
                        {
                            var handler = DataReceived;
                            if (handler != null)
                                handler(this, null);

                            delay = 25;
                        }
                        else
                        {
                            delay = 150;
                        }
                    }

                    Thread.Sleep(delay);
                }
            }
            catch (Exception ex)
            {
                logger.Log(this, ex);
            }

            Thread.Sleep(5000);

            logger.LogIfDebug(this, "Repeating loop...", LogLevels.Warning);
            }
        }

		public int Read(byte[] buffer, int offset, int count)
		{
            lock (port)
            {
                var readed = port.Read(buffer, offset, count);

                logger.LogIfDebug(this, string.Format("Readed {0} bytes", readed));

                return readed;
            }
		}
	}
}

