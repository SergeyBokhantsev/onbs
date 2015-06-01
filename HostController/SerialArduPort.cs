using Interfaces;
using System;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
namespace HostController
{
    [LogClass("SerialPort")]
	public class SerialArduPort : IPort
	{
        public event SerialDataReceivedEventHandler DataReceived;

        private SerialPort port;
        private readonly object portLocker = new object();
        private readonly ILogger logger;

        public SerialArduPort(ILogger logger, IConfig config)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");

            this.logger = logger;
           
            new System.Threading.Thread(() => Monitor(config)).Start();
        }

        private void Monitor(IConfig config)
        {
            Thread.CurrentThread.Name = "Serial";

            while (true)
            {
                int delay;

                try
                {
                    if (port == null)
                    {
                        var portEnabled = config.GetBool(Configuration.Names.ArduinoPortEnabled);
                        if (!portEnabled)
                        {
                            logger.Log(this, "Arduino port is disabled. Exit port loop.", LogLevels.Warning);
                            return;
                        }

                        var portPath = config.GetString(Configuration.Names.ArduinoPort);
                        var speed = config.GetInt(Configuration.Names.ArduinoPortSpeed);
                        var parity = (Parity)Enum.Parse(typeof(Parity), config.GetString(Configuration.Names.ArduinoPortParity));
                        var databits = config.GetInt(Configuration.Names.ArduinoPortDataBits);
                        var stopbits = (StopBits)Enum.Parse(typeof(StopBits), config.GetString(Configuration.Names.ArduinoPortStopBits));

                        lock (portLocker)
                        {
                            port = new SerialPort(portPath, speed, parity, databits, stopbits);
                            port.Open();
                        }
                    }

                    while (true)
                    {
                        lock (portLocker)
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

                    port = null;
                }

                Thread.Sleep(5000);

                logger.LogIfDebug(this, "Repeating loop...", LogLevels.Warning);
            }
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            lock (portLocker)
            {
                if (port != null)
                {
                    var readed = port.Read(buffer, offset, count);

                    logger.LogIfDebug(this, string.Format("Readed {0} bytes", readed));

                    return readed;
                }
                else
                    return 0;
            }
        }
	}
}

