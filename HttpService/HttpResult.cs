using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpServiceNamespace
{
    public abstract class HttpResult
    {
        public bool Success { get; private set; }

        public string ErrorMessage { get; private set; }

        public HttpStatusCode HttpCode { get; private set; }

        protected HttpResult(bool success, HttpStatusCode httpCode, string error)
        {
            Success = success;
            HttpCode = httpCode;
            ErrorMessage = error;
        }

        public Exception MakeException()
        {
            if (!Success)
                return new Exception(string.Format("HttpCode: {0}, Exception: {1}", HttpCode, ErrorMessage));
            else
                throw new InvalidOperationException("Cannot create exeption from successfull result");
        }
    }

    public class SimpleResult : HttpResult
    {
        public SimpleResult(bool success, HttpStatusCode code, string error)
            : base(success, code, error)
        {
        }
    }

    public class SimpleResult<T> : HttpResult
    {
        public T Value { get; private set; }

        public SimpleResult(bool success, HttpStatusCode httpCode, string error, T result)
            :base(success, httpCode, error)
        {
            Value = result;
        }
    }
}