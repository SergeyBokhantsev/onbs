using Interfaces;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Elm327
{
    public class Client : IDisposable
    {
        private SerialPort port;
        private readonly ILogger logger;

        private bool disposed;

        private byte[] inBuffer = new byte[4096];

        public Client(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");

            this.logger = logger;
        }

        public void Run(string portName)
        {
            port = new SerialPort(portName, 38400);
            port.Open();
        }

        public bool Reset()
        {
            if (disposed)
                return false;

            port.DiscardInBuffer();
            port.DiscardOutBuffer();

            if (Send("ATZ").Contains("ELM327 v1.5"))
            {
                if (Send("ATE0").Contains("OK"))
                {
                    logger.Log(this, "ELM327 module activated.", LogLevels.Debug);
                    return true;
                }
            }

            logger.Log(this, "Unable to activate ELM327 module.", LogLevels.Error);
            return false;
        }

        private string[] Send(string command)
        {
            try
            {
                if (disposed)
                    return new string[0];

                logger.LogIfDebug(this, string.Concat("Sending to ELM: ", command));

                port.Write(string.Concat(command, "\r\n"));

                int count = 0;
                int readTimeout = 15000;
                int waitInterval = 30;
                int waiting = 0;

                Thread.Sleep(50);

                while (waiting < readTimeout && !disposed)
                {
                    if (port.BytesToRead > 0)
                    {
                        waiting = 0;

                        count += port.Read(inBuffer, count, inBuffer.Length - count);

                        if (inBuffer[count - 1] == '>' || count == inBuffer.Length)
                            break;
                    }
                    else
                    {
                        Thread.Sleep(waitInterval);
                        waiting += waitInterval;
                    }
                }

                if (count > 0)
                {
                    var str = Encoding.Default.GetString(inBuffer, 0, count);
                    logger.LogIfDebug(this, string.Concat("Received from ELM: ", str));
                    var ret = str.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    return ret;
                }
                else
                {
                    logger.LogIfDebug(this, "Received from ELM: Nothing");
                    return new string[0];
                }
            }
            catch(Exception ex)
            {
                logger.Log(this, ex);
                return new string[0];
            }
        }

        public string[] Send(uint pid, string formatter, bool autoReset = true)
        {
			if (disposed)
				return null;

            var response = Send(pid.ToString(formatter));

            if (response.Contains("STOPPED")
                || response.Contains("UNABLE TO CONNECT")
                || response.Contains("NO DATA")
                || response.Any(l => l.StartsWith("7F")))
            {
                if (autoReset && Reset())
                {
                    return Send(pid, formatter, false);
                }
                else
                    return null;
            }

            return response;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                if (port != null)
                    port.Close();

                disposed = true;
            }
        }
    }
}
