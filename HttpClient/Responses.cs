using System;
using System.IO;
using System.Net;

namespace HttpClient
{
    public class ClientResponse : IDisposable
    {
        private HttpWebResponse webResponse;

        public HttpStatusCode Status { get; protected set; }
        public WebExceptionStatus WebExceptionStatus { get; protected set; }
        public string Error { get; protected set; }

        public WebHeaderCollection Headers
        {
            get
            {
                return webResponse != null ? webResponse.Headers : null;
            }
        }

        public ClientResponse(HttpWebResponse webResponse)
        {
            if (webResponse == null)
                throw new ArgumentNullException("webResponse");

            this.webResponse = webResponse;
            Status = webResponse.StatusCode;
            WebExceptionStatus = System.Net.WebExceptionStatus.Success;
            Error = webResponse.StatusDescription;
        }

        public ClientResponse(HttpWebResponse webResponse, WebExceptionStatus webExceptionStatus, string error)
        {
            this.webResponse = webResponse;
            WebExceptionStatus = webExceptionStatus;
            Status = webResponse != null ? webResponse.StatusCode : 0;
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
