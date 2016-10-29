using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServiceNamespace
{
    public class StringRequestWriter : IRequestWriter
    {
        private readonly byte[] body;

        public long Length
        {
            get { return body.Length; }
        }

        public StringRequestWriter(string bodyStr)
        {
            if (!string.IsNullOrEmpty(bodyStr))
            {
                body = HttpService.Encoding.GetBytes(bodyStr);
            }
        }

        public void Write(Stream stream)
        {
            stream.Write(body, 0, body.Length);
        }

        public async Task WriteAsync(Stream stream)
        {
            await stream.WriteAsync(body, 0, body.Length);
        }

        
    }
}
