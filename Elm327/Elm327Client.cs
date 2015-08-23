using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using Interfaces;

namespace Elm327
{
//    at?

//>AT Z?

//>AT Z


//ELM327 v1.5

//>AT
//?

//>010C
//SEARCHING...
//41 0C 00 00 

//>010C
//41 0C 00 00 

//>010c
//41 0C 00 00 

//>010c
//41 0C 00 00 

//>0100
//41 00 BE 3F B8 1C 

//>0105
//41 05 70 

//>010c
//41 0C 00 00 

//>ATI
//ELM327 v1.5

//>

    

    public class Elm327Client
    {
        public event Action<IElm327Response> ResponceReseived;

        private readonly SerialPort port;
        private readonly ILogger logger;
        private readonly IDispatcher dispatcher;
        private readonly byte[] buffer = new byte[512];
        private readonly StringBuilder inString = new StringBuilder();
        private readonly OBDProcessor processor = new OBDProcessor();

        public Elm327Client(string portName, ILogger logger, IDispatcher dispatcher)
        {
            if (string.IsNullOrEmpty(portName))
                throw new ArgumentNullException("portName");

            if (logger == null)
                throw new ArgumentNullException("logger");

            if (dispatcher == null)
                throw new ArgumentNullException("dispatcher");

            this.logger = logger;
            this.dispatcher = dispatcher;

            IDispatcherTimer timer = null;

            try
            {
                port = new SerialPort(portName, 38400);
                port.Open();

                timer = dispatcher.CreateTimer(200, CheckPort);
                timer.Enabled = true;
            }
            catch (Exception ex)
            {
                logger.Log(this, ex);
				                if (port != null)
                    port.Close();

                if (timer != null)
                    timer.Dispose();

                port = null;
            }
        }

        public void Request(Elm327FunctionTypes type)
        {
            if (port != null)
                port.Write(processor.CreateRequest(type));
        }

        private void CheckPort(object sender, EventArgs e)
        {
            if (port.BytesToRead > 0)
            {
                int readed = 0;

                do
                {
                    readed = port.Read(buffer, 0, buffer.Length);

                    for (int i = 0; i < readed; ++i)
                    {
						if (buffer[i] == 10 || buffer[i] == 13)
                        {
                            if (inString.Length > 0)
                            {
                                if (IsHexResponse(inString))
                                {
                                    var bytes = HexToBytes(inString.ToString());
                                    ProcessHexResponse(bytes);
                                }
                                else
                                {
                                    ProcessStringResponse(inString.ToString());
                                }

                                inString.Clear();
                            }
                        }
                        else
                        {
                            if (buffer[i] != 32) // Skipping spaces
                                inString.Append((char)buffer[i]);
                        }
                    }
                }
				while (false);
            }
        }

        private void ProcessStringResponse(string value)
        {
            OnResponseReseived(new Elm327Response<string>(Elm327FunctionTypes.RawString, value));
        }

        private void ProcessHexResponse(byte[] bytes)
        {
            OnResponseReseived(processor.GetResponse(bytes));
        }

        private void OnResponseReseived(IElm327Response response)
        {
            var handler = ResponceReseived;
            if (handler != null)
            {
                dispatcher.Invoke(this, null, (s, e) => handler(response));
            }
        }

        private bool IsHexResponse(StringBuilder inString)
        {
            for(int i = 0; i < inString.Length; ++i)
            {
                if (!(inString[i] >= 48 && inString[i] <= 57) 
                    && !(inString[i] >= 65 && inString[i] <= 70) 
                    && !(inString[i] >= 97 && inString[i] <= 102))
                    return false;
            }

            return true;
        }

        private byte[] HexToBytes(string str)
        {
            var ret = new byte[str.Length / 2];

            for (int i=0; i<str.Length; i+=2)
            {
                ret[i / 2] = Convert.ToByte(str.Substring(i, 2), 16);
            }

            return ret;
        }
    }
}
