using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpClient
{
    public class ClientResponse : IDisposable
    {
        private HttpWebResponse webResponse;

        public HttpStatusCode Status { get; protected set; }

        public string Error { get; protected set; }

        public ClientResponse(HttpWebResponse webResponse)
        {
            if (webResponse == null)
                throw new ArgumentNullException("webResponse");

            this.webResponse = webResponse;
            Status = webResponse.StatusCode;
            Error = webResponse.StatusDescription;
        }

        public ClientResponse(HttpStatusCode status, string error)
        {
            Status = status;
            Error = error;
        }

        public Stream GetStream()
        {
            return webResponse != null ? webResponse.GetResponseStream() : null;
        }

        public void Dispose()
        {
            if (webResponse != null)
            {
                webResponse.Dispose();
                webResponse = null;
            }
        }
    }
}
