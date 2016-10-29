using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Threading;
using System.Net.Http;

namespace HttpServiceNamespace
{
    public class HttpService
    {
        public static readonly Encoding Encoding = Encoding.UTF8;

        private readonly IAuthorizationProvider authorizationProvider;
        private readonly CookieContainer cookieContainer = new CookieContainer();

        private int requestCount;
        private Stopwatch requestStopwatch = new Stopwatch();
        private int isBusy;

        private readonly object currentRequestLocker = new object();
        private HttpWebRequest currentRequest;

        public IHttpDumper HttpDumper { get; private set; }
        public TimeSpan HttpGetTimeout { get; set; }
        public TimeSpan HttpPostTimeout { get; set; }

        /// <summary>
        /// Gets the authorization provider.
        /// </summary>
        public IAuthorizationProvider AuthorizationProvider { get { return authorizationProvider; } }

        public HttpService(IAuthorizationProvider authorizationProvider, IHttpDumper httpDumper)
        {     
            this.authorizationProvider = authorizationProvider;
            this.HttpDumper = httpDumper;

            HttpGetTimeout = HttpPostTimeout = new TimeSpan(0, 0, 180);
        }

        private void Lock(HttpWebRequest request)
        {
            if (Interlocked.Exchange(ref isBusy, 1) == 1)
                throw new InvalidOperationException("Concurrent calls to OData service is not supported");

            if (null != currentRequest)
                throw new InvalidOperationException("Current request is not null");

            if (null == request)
                throw new ArgumentNullException("request");

            lock (currentRequestLocker)
            {
                currentRequest = request;
            }
        }

        private void Unlock()
        {
            lock (currentRequestLocker)
            {
                currentRequest = null;
            }

            Interlocked.Exchange(ref isBusy, 0);
        }

        public void Abort()
        {
            lock (currentRequestLocker)
            {
                if (null != currentRequest)
                    currentRequest.Abort();
            }
        }

        public T Execute<T>(ExecuteArguments<T> args)
            where T : HttpResult
        {
            if (null == args)
                throw new ArgumentNullException("args");

            Lock(args.Request);

            requestCount++;
            requestStopwatch.Restart();

            try
            {
                try
                {
                    AuthorizeRequest(args.Request).Wait();
                }
                catch (AggregateException ex)
                {
                    if (null != ex.InnerException)
                        throw ex.InnerException;
                    else
                        throw;
                }
                finally     // Make sure BeginDump is called even if AuthorizeRequest throws
                {
                    BeginDump(args.Request);
                }

                if (null != args.Writer)
                {
                    using (var requestStream = args.Request.GetRequestStream())
                    {
                        args.Writer.Write(WrapToDumping(requestStream));
                    }
                }

                using (var response = args.Request.GetResponse() as HttpWebResponse)
                {
                    return ProcessResponse(args.Reader, response).GetAwaiter().GetResult();
                }
            }
            catch (WebException ex)
            {
                return ProcessWebException(ex, args.Reader);
            }
            catch (Exception ex)
            {
                ExceptionDump(ex);
                return args.Reader.FromException(ex.Message, HttpStatusCode.Unused);
            }
            finally
            {
                requestStopwatch.Stop();
                EndDump(requestStopwatch.ElapsedMilliseconds);

                Unlock();
            }
        }

        public async Task<T> ExecuteAsync<T>(ExecuteArguments<T> args)
            where T : HttpResult
        {
            if (null == args)
                throw new ArgumentNullException("args");

            Lock(args.Request);

            requestCount++;
            requestStopwatch.Restart();

            try
            {
                try
                {
                    await AuthorizeRequest(args.Request).ConfigureAwait(false);
                }
                finally     // Make sure BeginDump is called even if AuthorizeRequest throws
                {
                    BeginDump(args.Request);
                }

                if (null != args.Writer)
                {
                    using (var requestStream = await AsyncHttpOperationWithTimeout(args.Request.GetRequestStreamAsync(), args.Request)
                        .ConfigureAwait(false))
                    {
                        await args.Writer.WriteAsync(WrapToDumping(requestStream));
                    }
                }

                using (var response = await AsyncHttpOperationWithTimeout(args.Request.GetResponseAsync(), args.Request).ConfigureAwait(false))
                {
                    return await ProcessResponse(args.Reader, response as HttpWebResponse);
                }
            }
            catch (WebException ex)
            {
                return ProcessWebException(ex, args.Reader);
            }
            catch (AggregateException ex)
            {
                var flattenException = ex.Flatten();

                var webException = flattenException.InnerException as WebException;

                if (null != webException)
                    return ProcessWebException(webException, args.Reader);
                else
                {
                    ExceptionDump(ex);
                    throw flattenException.InnerException;
                }
            }
            catch (Exception ex)
            {
                ExceptionDump(ex);
                return args.Reader.FromException(ex.Message, HttpStatusCode.Unused);
            }
            finally
            {
                requestStopwatch.Stop();
                EndDump(requestStopwatch.ElapsedMilliseconds);

                Unlock();
            }
        }

        private async Task<T> AsyncHttpOperationWithTimeout<T>(Task<T> httpOperation, HttpWebRequest request)
            where T: class
        {
            var cts = new CancellationTokenSource();

            var timeoutTask = Task.Delay((request.Method.Equals("GET", StringComparison.InvariantCultureIgnoreCase)) ? HttpGetTimeout : HttpPostTimeout, cts.Token);
            
            await Task.WhenAny(httpOperation, timeoutTask).ConfigureAwait(false);

            cts.Cancel();

            if (!httpOperation.IsCompleted)
            {
                request.Abort();

                try
                {
                    // Waiting request to abort.
                    // This could throw (if Abort method had interrupted the operation)
                    // Or could not throw, if Abort was called too late.
                    // Anyway it is not safe to use the result after Abort, so just dispose the result
                    var disposableResult = (await httpOperation.ConfigureAwait(false)) as IDisposable;

                    if (null != disposableResult)
                    {
                        try
                        {
                            disposableResult.Dispose();
                        }
                        catch
                        {
                            //Do nothing
                        }
                    }

                    throw new WebException("HTTP operation timeout", WebExceptionStatus.Timeout);
                }
                catch (Exception ex)
                {
                    throw new WebException(ex.Message, ex, WebExceptionStatus.Timeout, null);
                }
            }

            return httpOperation.Result;
        }

        private async Task<T> ProcessResponse<T>(IResultReader<T> reader, HttpWebResponse response)
            where T : HttpResult
        {
            BeginDump(response);

            using (var responseStream = response.GetResponseStream())
            {
                return await reader.FromResponse(response.StatusCode, response.Headers, WrapToDumping(responseStream));
            }
        }

        private T ProcessWebException<T>(WebException ex, IResultReader<T> reader)
            where T : HttpResult
        {
            ExceptionDump(ex);

            string errorMessage = ex.Message;
            var statusCode = HttpStatusCode.Unused;

            var response = ex.Response as HttpWebResponse;

            if (null != response)
            {
                BeginDump(response);

                statusCode = response.StatusCode;

                try
                {
                    using (var stream = WrapToDumping(response.GetResponseStream()))
                    {
                        using (var sr = new StreamReader(stream))
                        {
                            var buffer = new char[512];
                            int len = sr.ReadBlock(buffer, 0, buffer.Length);

                            if (len > 0)
                                errorMessage = new String(buffer, 0, len);
                        }
                    }
                }
                catch (Exception exception)
                {
                    errorMessage = string.Format("Exception '{0}' while processing WebEexeption: {1}", exception.Message, ex.Message);
                }
            }

            return reader.FromException(errorMessage, statusCode);
        }

        private async Task AuthorizeRequest(HttpWebRequest request)
        {
            if (request.Method.Equals("GET", StringComparison.InvariantCultureIgnoreCase))
                request.Timeout = (int)HttpGetTimeout.TotalMilliseconds;
            else
                request.Timeout = (int)HttpPostTimeout.TotalMilliseconds;

            if (null != authorizationProvider)
                await authorizationProvider.Authorize(request);
        }

        // from ExchangeTransientErrorDetectionStrategy
        private T GetInnerOrSelf<T>(Exception ex) where T : Exception
        {
            while (ex != null)
            {
                T result = ex as T;
                if (result != null)
                    return result;
                ex = ex.InnerException;
            }
            return null;
        }


        #region HTTP DUMP
        private IEnumerable<KeyValuePair<string, string>> ExtractHeaders(WebHeaderCollection headers)
        {
            if (null != headers && headers.Count > 0)
            {
                var names = headers.AllKeys;
                return names.Select(name => new KeyValuePair<string, string>(name, headers.Get(name)));
            }
            else
            {
                return new KeyValuePair<string, string>[0];
            }
        }

        private Stream WrapToDumping(Stream stream)
        {
            if (null != HttpDumper)
            {
                return new DumpingStream(stream, HttpDumper, requestCount);
            }
            else
                return stream;
        }

        private void BeginDump(HttpWebRequest request)
        {
            if (null != HttpDumper)
            {
                if (null == request)
                    throw new ArgumentNullException("request");

				HttpDumper.BeginRequest(requestCount, request.RequestUri, request.Method, ExtractHeaders(request.Headers));
            }
        }

        private void BeginDump(HttpWebResponse response)
        {
            if (null != HttpDumper)
            {
                if (null == response)
                    throw new ArgumentNullException("response");

				HttpDumper.BeginResponse(requestCount, response.ResponseUri, response.StatusCode, ExtractHeaders(response.Headers));
            }
        }

        private void ExceptionDump(Exception ex)
        {
            if (null != HttpDumper)
            {
				HttpDumper.Exception(requestCount, ex);
            }
        }

        private void EndDump(long elapsedMilliseconds)
        {
            if (null != HttpDumper)
            {
				HttpDumper.End(requestCount, elapsedMilliseconds);
            }
        }
        #endregion
    }
}
