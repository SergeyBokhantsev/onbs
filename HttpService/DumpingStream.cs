using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServiceNamespace
{
    public class DumpingStream : Stream
    {
        private readonly Stream stream;
        private readonly IHttpDumper dumper;
        private readonly int requestId;

        public DumpingStream(Stream stream, IHttpDumper dumper, int requestId)
        {
            this.stream = stream;
            this.dumper = dumper;
            this.requestId = requestId;

            if (null == this.stream)
                throw new ArgumentNullException("stream");

            if (null == this.dumper)
                throw new ArgumentNullException("dumper");
        }

        public override bool CanRead
        {
            get { return stream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return stream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return stream.CanWrite; }
        }

        public override void Flush()
        {
            stream.Flush();
        }

        public override long Length
        {
            get { return stream.Length; }
        }

        public override long Position
        {
            get
            {
                return stream.Position;
            }
            set
            {
                stream.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var readed = stream.Read(buffer, offset, count);
            Dump(buffer, offset, readed);
            return readed;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Dump(buffer, offset, count);
            stream.Write(buffer, offset, count);
        }

        private void Dump(byte[] buffer, int offset, int count)
        {
            dumper.AppendBody(requestId, buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                stream.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
