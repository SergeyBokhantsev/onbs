using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServiceNamespace
{
    public interface IRequestWriter
    {
        long Length { get; }

        void Write(Stream stream);

        Task WriteAsync(Stream stream);
    }
}
