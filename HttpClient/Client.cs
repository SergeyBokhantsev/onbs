using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace HttpClient
{
    public class Client
    {
        public event Action<Exception> ClientException;

        public event Action<HttpWebRequest> BeforeSendRequest;

        public ICollection<KeyValuePair<HttpRequestHeader, string>> Headers
        { 
            get; 
            private set; 
        }

        public Client()
        {
            Headers = new List<KeyValuePair<HttpRequestHeader, string>>();
        }

        public ClientResponse Get(Uri uri, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null)
        {
            var request = InsertHeaders(WebRequest.Create(uri) as HttpWebRequest, headers);
            OnBeforeSendRequest(request);
            return ExecuteRequest(request);
        }

        public ClientResponse Get(Uri uri, int retryCount, int retryDelay, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null)
        {
            return ExecuteWithTransientConnectionErrorRetry(() => Get(uri, headers), retryCount, retryDelay);
        }

        public ClientResponse Post(Uri uri, string body, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null)
        {
            var request = InsertHeaders(WebRequest.Create(uri) as HttpWebRequest, headers);
            request.Method = "POST";
            OnBeforeSendRequest(request);
            return ExecuteRequest(request, body);
        }

        public ClientResponse Post(Uri uri, string body, int retryCount, int retryDelay, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null)
        {
            return ExecuteWithTransientConnectionErrorRetry(() => Post(uri, body, headers), retryCount, retryDelay);
        }

        public ClientResponse Put(Uri uri, string body, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null)
        {
            var request = InsertHeaders(WebRequest.Create(uri) as HttpWebRequest, headers);
            request.Method = "PUT";
            OnBeforeSendRequest(request);
            return ExecuteRequest(request, body);
        }

        public ClientResponse Put(Uri uri, string body, int retryCount, int retryDelay, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null)
        {
            return ExecuteWithTransientConnectionErrorRetry(() => Put(uri, body, headers), retryCount, retryDelay);
        }

        public ClientResponse Delete(Uri uri, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null)
        {
            var request = InsertHeaders(WebRequest.Create(uri) as HttpWebRequest, headers);
            request.Method = "DELETE";
            OnBeforeSendRequest(request);
            return ExecuteRequest(request);
        }

        private HttpWebRequest InsertHeaders(HttpWebRequest request, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            if (Headers.Any())
            {
                foreach (var pair in Headers)
                {
                    request.Headers.Add(pair.Key, pair.Value);
                }
            }

            if (headers != null)
            {
                var pairs = headers as KeyValuePair<HttpRequestHeader, string>[] ?? headers.ToArray();

                foreach(var pair in pairs)
                {
                    request.Headers.Add(pair.Key, pair.Value);
                }
            }

            return request;
        }

        private ClientResponse ExecuteRequest(HttpWebRequest request, string body = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(body))
                {
                    var data = Encoding.Default.GetBytes(body);
                    request.ContentLength = data.Length;
                    
                    using (var stream = request.GetRequestStream())
                    {
                        for (int i = 0; i < data.Length; ++i)
                            stream.WriteByte(data[i]);
                    }
                }
                else
                {
                    request.ContentLength = 0;
                }

                var response = request.GetResponse() as HttpWebResponse;
                return new ClientResponse(response);
            }
            catch (WebException ex)
            {
                OnException(ex);

                var httpResponse = ex.Response as HttpWebResponse;

                return new ClientResponse(httpResponse, ex.Status, ex.Message);
            }
            catch (Exception ex)
            {
                OnException(ex);

                return new ClientResponse(null, WebExceptionStatus.UnknownError, ex.Message);
            }
        }

        private ClientResponse ExecuteWithTransientConnectionErrorRetry(Func<ClientResponse> func, int retries, int delay)
        {
            ClientResponse response = null;

            while (retries-- > 0)
            {
                response = func();

                switch (response.WebExceptionStatus)
                {
                    case WebExceptionStatus.ConnectFailure:
                    case WebExceptionStatus.ConnectionClosed:
                    case WebExceptionStatus.NameResolutionFailure:
                    case WebExceptionStatus.PipelineFailure:
                    case WebExceptionStatus.ProxyNameResolutionFailure:
                    case WebExceptionStatus.ReceiveFailure:
                    case WebExceptionStatus.SecureChannelFailure:
                    case WebExceptionStatus.SendFailure:
                    case WebExceptionStatus.Timeout:
                        OnException(new Exception(string.Concat("Repeating transient error event. WebExceptionStatus was: ", response.WebExceptionStatus)));
                        Thread.Sleep(delay);
                        response.Dispose();
                        continue;

                    case WebExceptionStatus.ProtocolError:
                        switch (response.Status)
                        {
                            case HttpStatusCode.GatewayTimeout:
                            case HttpStatusCode.InternalServerError:
                            case HttpStatusCode.RequestTimeout:
                            case HttpStatusCode.ServiceUnavailable:
                            case HttpStatusCode.BadGateway:
                                OnException(new Exception(string.Concat("Repeating transient error event. HttpStatus was: ", response.Status)));
                                Thread.Sleep(delay);
                                response.Dispose();
                                continue;
                        }
                        break;
                }

                break;
            }

            return response;
        }

        private void OnBeforeSendRequest(HttpWebRequest request)
        {
            var handler = BeforeSendRequest;
            if (handler != null)
                handler(request);
        }

        private void OnException(Exception ex)
        {
            var handler = ClientException;
            if (handler != null)
                handler(ex);
        }
    }
}
