using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.SerialTransportProtocol
{
    public interface IFramesAcceptor
    {
        STPFrame.Types FrameType { get; }
        void AcceptFrames(IEnumerable<STPFrame> frames);
    }
}
