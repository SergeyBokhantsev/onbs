﻿using System;
using System.IO;
using System.Net;

namespace HttpClient
{
    public class ClientResponse : IDisposable
    {
        private HttpWebResponse webResponse;

        public HttpStatusCode Status { get; protected set; }

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
