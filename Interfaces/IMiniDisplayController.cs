using Interfaces.SerialTransportProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IMiniDisplayController : IController, IFrameProvider
    {
        void Update();
        void Fill();
        void Cls();
    }
}
