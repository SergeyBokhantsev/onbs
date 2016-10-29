using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpServiceNamespace
{
    public interface IHttpDumper
    {
        void BeginRequest(int id, Uri uri, string method, IEnumerable<KeyValuePair<string, string>> headers);
        void BeginResponse(int id, Uri uri, HttpStatusCode statusCode, IEnumerable<KeyValuePair<string, string>> headers);
        void AppendBody(int id, byte[] buffer, int offset, int count);
        void Exception(int id, Exception ex);
        void End(int id, long elapsedMilliseconds);
    }
}
