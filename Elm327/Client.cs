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

        protected void Run(string portName)
        {
            port = new SerialPort(portName, 38400);
            port.Open();
        }

        protected bool Reset()
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

        protected byte[] FirstHexString(string[] response)
        {
            if (response != null)
            {
                foreach (var line in response)
                {
                    if (IsHexString(line))
                    {
                        return HexToBytes(line);
                    }
                }
            }

            return null;
        }

        protected string[] Send(uint pid, string formatter, bool autoReset = true)
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

        private bool IsHexString(string str)
        {
            for (int i = 0; i < str.Length; ++i)
            {
                if (!(str[i] >= 48 && str[i] <= 57)
                    && !(str[i] >= 65 && str[i] <= 70)
                    && !(str[i] >= 97 && str[i] <= 102)
                    && str[i] != 32)
                    return false;
            }

            return true;
        }

        private byte[] HexToBytes(string str)
        {
            str = str.Replace(" ", string.Empty);

            logger.LogIfDebug(this, string.Concat("Begin converting to HEX: ", str));

            var ret = new byte[str.Length / 2];

            for (int i = 0; i < str.Length; i += 2)
            {
                ret[i / 2] = Convert.ToByte(str.Substring(i, 2), 16);
            }

            logger.LogIfDebug(this, string.Concat("Resulting bytes: ", string.Join(", ", ret)));

            return ret;
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
