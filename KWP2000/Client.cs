using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interfaces;

namespace KWP2000
{
    public class Client
    {
        private readonly SerialPort port;
        private readonly ILogger logger;

        private readonly byte[] F0 = new byte[] { 240 };
        private readonly byte[] StartSession = new byte[] { 0x81, 0x10, 0xf1, 0x81, 3 };

        private readonly byte[2048] buffer;

        private bool Initialized;

        public Client(SerialPort port, ILogger logger)
        {
            if (port == null)
                throw new ArgumentNullException("port");

            if (logger == null)
                throw new ArgumentNullException("logger");

            this.port = port;
            this.logger = logger;
        }

        private void WorkLoop()
        {

            if (!Initialized)
            {

            }
        }

        private bool FastInitECU()
        {
            try
            {
                if (!port.IsOpen)
                {
                    port.Open();
                }

                port.DiscardInBuffer();
                port.DiscardOutBuffer();

                port.BaudRate = 200;
                port.Write(F0, 0, F0.Length);

                port.BaudRate = 10400;
                port.Write(StartSession, 0, StartSession.Length);
                

                var inCount = SleepAndRead(100);



                return true;
            }
            catch (Exception ex)
            {
                logger.Log(this, ex);
                return false;
            }
        }

        private int SleepAndRead(int ms)
        {
            Thread.Sleep(100);

            if (port.BytesToRead > buffer.Length)
            {
                logger.Log(this, "Buffer overflow on read operation", LogLevels.Warning);
                return 0;
            }

            return port.Read(buffer, 0, buffer.Length);
        }
    }
}
