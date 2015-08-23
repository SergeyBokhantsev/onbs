using Elm327;
using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elm327Controller
{
    public class Elm327Controller : IElm327Controller
    {
        private readonly IHostController hc;
        private Elm327Client elmClient;

        public event Action<IElm327Response> ResponceReseived
        {
            add
            {
                EnsureClient();

                if (elmClient != null)
                {
                    elmClient.ResponceReseived += value;
                    hc.Logger.Log(this, "ResponceReseived handler registered.", LogLevels.Info);
                }
            }
            remove
            {
                if (elmClient != null)
                {
                    elmClient.ResponceReseived -= value;
                    hc.Logger.Log(this, "ResponceReseived handler unregistered.", LogLevels.Info);
                }
            }
        }

        public Elm327Controller(IHostController hc)
        {
            if (hc == null)
                throw new ArgumentNullException("hc");

            this.hc = hc;
        }

        private void EnsureClient()
        {
            if (elmClient != null)
                return;

            try
            {
                var portName = hc.Config.GetString(ConfigNames.Elm327Port);
                elmClient = new Elm327Client(portName, hc.Logger, hc.Dispatcher);
                hc.Logger.Log(this, "Elm327 controller created.", LogLevels.Info);
            }
            catch (Exception ex)
            {
                hc.Logger.Log(this, ex);
            }
        }

        public void Request(Elm327FunctionTypes type)
        {
            if (elmClient != null)
            {
                elmClient.Request(type);
            }
        }
    }
}
