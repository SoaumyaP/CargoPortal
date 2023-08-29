using System;
using System.Net;
using System.Net.Http.Headers;

namespace Groove.SP.Infrastructure.CSFE
{
    public class ServiceException : Exception
    {
        public ServiceException(string message, Exception innerException = null)
            : this(message, responseHeaders: null, statusCode: default(System.Net.HttpStatusCode), innerException: innerException)
        {
        }

        public ServiceException(string message,
                                HttpResponseHeaders responseHeaders, 
                                HttpStatusCode statusCode, 
                                Exception innerException = null)
            : base(message, innerException)
        {
            this.ResponseHeaders = responseHeaders;
            this.StatusCode = statusCode;
        }

        public ServiceException(string message,
                                HttpResponseHeaders responseHeaders,
                                HttpStatusCode statusCode,
                                string rawResponseBody,
                                Exception innerException = null)
            : this(message, responseHeaders, statusCode, innerException)
        {
            this.RawResponseBody = rawResponseBody;
        }

        public HttpResponseHeaders ResponseHeaders { get; }

        public HttpStatusCode StatusCode { get; }

        public string RawResponseBody { get; }
    }
}
