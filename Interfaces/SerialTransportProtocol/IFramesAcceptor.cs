using System.Collections.Generic;

namespace Interfaces.SerialTransportProtocol
{
    public interface IFramesAcceptor
    {
        STPFrame.Types FrameType { get; }
        void AcceptFrames(IEnumerable<STPFrame> frames);
    }
}
