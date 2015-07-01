using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpClient
{
    public class Client
    {
        public event Action<Exception> ClientException;

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
            return ExecuteRequest(InsertHeaders(WebRequest.CreateHttp(uri), headers));
        }

        public ClientResponse Post(Uri uri, string body, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null)
        {
            var request = InsertHeaders(WebRequest.CreateHttp(uri), headers);

            request.Method = "POST";

            return ExecuteRequest(request, body);
        }

        public ClientResponse Delete(Uri uri, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null)
        {
            var request = InsertHeaders(WebRequest.CreateHttp(uri), headers);

            request.Method = "DELETE";

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

            if (headers != null && headers.Any())
            {
                foreach( var pair in headers)
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

                if (httpResponse != null)
                    return new ClientResponse(httpResponse.StatusCode, ex.Message);
                else
                    return new ClientResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            catch (Exception ex)
            {
                OnException(ex);

                return new ClientResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private void OnException(Exception ex)
        {
            var handler = ClientException;
            if (handler != null)
                handler(ex);
        }
    }
}
