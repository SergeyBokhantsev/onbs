using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServiceNamespace
{
    public static class Extensions
    {
        public static async Task<MemoryStream> ToMemoryStreamAsync(this Stream webStream)
        { 
            var ms = new MemoryStream();
            var buffer = new byte[1024];
            int readed = 0;
            do
            {
                readed = await webStream.ReadAsync(buffer, 0, buffer.Length);
                ms.Write(buffer, 0, readed);
            }
            while (readed > 0);

            ms.Seek(0, SeekOrigin.Begin);

            return ms;
        }
    }
}
