using System;

namespace Interfaces.SerialTransportProtocol
{
    public delegate void FrameToSendDelegate(STPFrame frame);

    public interface IFrameProvider
    {
        event FrameToSendDelegate FrameToSend;
    }
}
