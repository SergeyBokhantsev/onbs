using System.Collections.Generic;

namespace Interfaces.SerialTransportProtocol
{
    public interface ISTPCodec
    {
        List<STPFrame> Decode(IPort port);
        List<STPFrame> Decode(IEnumerable<STPFrame> frames);
        byte[] Encode(STPFrame frame);
    }
}
