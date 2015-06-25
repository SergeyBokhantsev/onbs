using Interfaces;
using System;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
namespace HostController
{
	public class SerialArduPort : IPort
	{
        public event SerialDataReceivedEventHandler DataReceived;

        private SerialPort port;
        private readonly object portLocker = new object();
        private readonly ILogger logger;

        private long readedCount;

        public long OverallReadedBytes
        {
            get 
            { 
                return Interlocked.Read(ref readedCount); 
            }
        }

        public SerialArduPort(ILogger logger, IConfig config)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");

            this.logger = logger;
           
            var t = new System.Threading.Thread(() => Monitor(config));
			t.IsBackground = true;
			t.Start ();
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
                        var portEnabled = config.GetBool(ConfigNames.ArduinoPortEnabled);
                        if (!portEnabled)
                        {
                            logger.Log(this, "Arduino port is disabled. Exit port loop.", LogLevels.Warning);
                            return;
                        }

                        var portPath = config.GetString(ConfigNames.ArduinoPort);
                        var speed = config.GetInt(ConfigNames.ArduinoPortSpeed);
                        var parity = (Parity)Enum.Parse(typeof(Parity), config.GetString(ConfigNames.ArduinoPortParity));
                        var databits = config.GetInt(ConfigNames.ArduinoPortDataBits);
                        var stopbits = (StopBits)Enum.Parse(typeof(StopBits), config.GetString(ConfigNames.ArduinoPortStopBits));

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

                    Interlocked.Add(ref readedCount, readed);

                    logger.LogIfDebug(this, string.Format("Readed {0} bytes", readed));

                    return readed;
                }
                else
                    return 0;
            }
        }
    }
}

