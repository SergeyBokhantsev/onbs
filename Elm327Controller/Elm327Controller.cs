using Elm327;
using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elm327Controller
{
    public class Elm327Controller : IElm327Controller, IDisposable
    {
        private readonly object locker = new object();
        private readonly IHostController hc;
        private Elm327.Client elm;
        private bool disposed;
        
        public string Error { get; private set; }        

        public Elm327Controller(IHostController hc)
        {
            this.hc = hc;
        }

        public void Reset()
        {
            lock (locker)
            {
                if (elm != null)
                    elm.Dispose();

                Error = null;
                elm = null;
            }
        }

        private bool EnsureElm()
        {
            if (Error != null)
            {
                return false;
            }
            else if (disposed)
            {
                Error = "Controller is disposed!";
                return false;
            }
            else if (elm == null)
            {
                try
                {
					hc.GetController<IArduinoController>().RelayService.Disable(Interfaces.Relays.Relay.OBD); 
					System.Threading.Thread.Sleep(500);
                    hc.GetController<IArduinoController>().RelayService.Enable(Interfaces.Relays.Relay.OBD);

                    var portName = hc.Config.GetString(ConfigNames.Elm327Port);

                    hc.Logger.Log(this, string.Concat("ELM327 port resolved as ", portName), LogLevels.Info);

                    elm = new Client(hc.Logger);
                    elm.Run(portName);

                    if (elm.Reset())
                        return true;
                    else
                    {
                        Error = "Unable to reset Elm327 module";
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    hc.Logger.Log(this, ex);
                    Error = ex.Message;
                    return false;
                }
            }
            else
                return true;
        }

        public Nullable<T> GetPIDValue<T>(uint pid, int expectedBytesCount, Func<byte[], T> formula)
            where T: struct
        {
            var bytes = GetPIDValue(pid);

            if (bytes != null && bytes.Length == expectedBytesCount)
                return formula(bytes);
            else
                return null;
        }

        public byte[] GetPIDValue(uint pid)
        {
            byte[] result = null;

            lock (locker)
            {
                if (EnsureElm())
                {
                    result = BitHelper.FirstHexString(elm.Send(0x0100 + pid, "X4"));
                }
            }

            return result;
        }

        public IEnumerable<string> GetTroubleCodeFrames()
        {
            lock (locker)
            {
                if (EnsureElm())
                {
                    return BitHelper.AllHexStrings(elm.Send(0x03, "X2"));
                }
                else
                {
                    return null;
                }
            }
        }

        public bool ResetTroubleCodes()
        {
            lock (locker)
            {
                var result = BitHelper.FirstHexString(elm.Send(0x04, "X2"));

                return result != null && result.Any() && result.First() == (byte)0x44;
            }
        }

        public void Dispose()
        {
            lock (locker)
            {
                if (!disposed)
                {
                    disposed = true;
                    Reset();

					hc.GetController<IArduinoController>().RelayService.Disable(Interfaces.Relays.Relay.OBD);
                }
            }
        }
    }
}
