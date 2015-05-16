using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialTransportProtocol
{
    internal static class Extensions
    {
        public static void CopyTo(this byte[] source, byte[] dest, int destIndex, int count)
        {
            for (int i = 0; i < count; ++i)
            {
                dest[destIndex + i] = source[i];
            }
        }
    }
}
