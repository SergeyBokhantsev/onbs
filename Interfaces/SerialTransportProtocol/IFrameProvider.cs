using System;

namespace Interfaces.SerialTransportProtocol
{
    public interface IFrameProvider
    {
        event Action<STPFrame> FrameToSend;
    }
}
