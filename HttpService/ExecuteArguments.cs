using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpServiceNamespace
{
    public class ExecuteArguments<T>
            where T : HttpResult
    {
        public HttpWebRequest Request { get; private set; }
        public IResultReader<T> Reader { get; private set; }
        public IRequestWriter Writer { get; private set; }

        private static HttpWebRequest CreateRequest(Uri uri)
        {
            if (null == uri)
                throw new ArgumentNullException("uri");

            return WebRequest.Create(uri) as HttpWebRequest;
        }

        public static ExecuteArguments<T> CreateGET<T>(Uri uri, IResultReader<T> reader)
            where T : HttpResult
        {
            return Create(uri, "GET", reader, null);
        }

        public static ExecuteArguments<T> Create<T>(Uri uri, string method, IResultReader<T> reader, IRequestWriter writer)
            where T : HttpResult
        {
            var request = CreateRequest(uri);
            request.Method = method;
            request.ContentLength = null != writer ? writer.Length : 0;
            return new ExecuteArguments<T>(request, reader, writer);
        }

        public ExecuteArguments(HttpWebRequest request, IResultReader<T> reader, IRequestWriter writer = null)
        {
            if (null == request)
                throw new ArgumentNullException("request");

            if (null == reader)
                throw new ArgumentNullException("reader");

            Request = request;
            Reader = reader;
            Writer = writer;
        }
    }
}
