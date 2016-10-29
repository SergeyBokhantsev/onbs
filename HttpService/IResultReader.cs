using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpServiceNamespace
{
    public interface IResultReader<T>
        where T : HttpResult
    {
        Task<T> FromResponse(HttpStatusCode statusCode, WebHeaderCollection headers, Stream responseStream);
        T FromException(string errorMessage, HttpStatusCode httpCode);
    }
}
