using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.SerialTransportProtocol
{
    public class IFrameProvider
    {
        event Action<STPFrame> FrameToSend;
    }
}
