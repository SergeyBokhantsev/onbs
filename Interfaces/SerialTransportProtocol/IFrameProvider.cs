using System;

namespace Interfaces.SerialTransportProtocol
{
    public delegate void FrameToSendDelegate(STPFrame frame, int delayAfterSend);

    public interface IFrameProvider
    {
        event FrameToSendDelegate FrameToSend;
    }
}
