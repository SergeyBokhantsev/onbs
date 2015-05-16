using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.SerialTransportProtocol
{
    public interface ISTPCodec
    {
        List<STPFrame> Decode(Stream stream);
        List<STPFrame> Decode(IEnumerable<STPFrame> frames);
        byte[] Encode(STPFrame frame);
    }
}
