using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Net;
using System.Dynamic;

namespace Groove.Infrastructure.APIClientCore
{
    public class CollectionRequest
    {
        private ResponseHandler responseHandler;

        public CollectionRequest(
            string requestUrl,
            BaseClient client,
            IDictionary<string, string> headers = null)
        {
            this.Method = "GET";
            this.Client = client;
            this.responseHandler = new ResponseHandler();
            this.RequestUrl = requestUrl;
            this.Headers = headers;
        }

        public BaseClient Client { get; private set; }
        public string Method { get; set; }
        public string RequestUrl { get; internal set; }
        public string ContentType { get; set; }
        public IDictionary<string,string> Headers { get; private set; }

        public async Task<T> SendAsync<T>(
            object serializableObject) where T: ResponseResultBase
        {
            using (var response = await this.SendRequestAsync(serializableObject).ConfigureAwait(false))
            {
                return await this.responseHandler.HandleResponse<T>(response);
            }
        }

        /// <summary>
        /// It is to send silent request.
        /// In case there is exception, it will be wrapped into response with some information
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializableObject"></param>
        /// <returns></returns>
        public async Task<T> SendSilentAsync<T>(
           object serializableObject) where T : ResponseResultBase
        {
            using (var response = await this.SendRequestSilentAsync(serializableObject).ConfigureAwait(false))
            {
                return await this.responseHandler.HandleResponseSilent<T>(response);
            }
        }

        public HttpRequestMessage GetHttpRequestMessage()
        {           
            var request = new HttpRequestMessage(new HttpMethod(this.Method), this.RequestUrl);
            this.AddHeadersToRequest(request);
            return request;
        }

        private void AddHeadersToRequest(HttpRequestMessage request)
        {
            if (this.Headers != null)
            {
                foreach (var header in this.Headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        public async Task<HttpResponseMessage> SendRequestAsync(
            object serializableObject)
        {
            using (var request = this.GetHttpRequestMessage())
            {
                if (serializableObject != null)
                {
                    request.Content = new StringContent(JsonConvert.SerializeObject(serializableObject));

                    if (!string.IsNullOrEmpty(this.ContentType))
                    {
                        request.Content.Headers.ContentType = new MediaTypeHeaderValue(this.ContentType);
                    }
                }
                try
                {
                    var response = await this.Client.HttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
                    return response;
                }
                catch (TaskCanceledException ex)
                {
                    throw new TaskCanceledException(ErrorConstants.Codes.Timeout, ex.InnerException);
                }
                catch (Exception ex)
                {
                    throw new Exception(ErrorConstants.Codes.GeneralException, ex.InnerException);
                }
            }
        }

        /// <summary>
        /// It is to send silent request.
        /// In case there is exception, it will be wrapped into response with content is exception information and status code is 400
        /// </summary>
        /// <param name="serializableObject"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> SendRequestSilentAsync(object serializableObject)
        {
            using (var request = this.GetHttpRequestMessage())
            {
                if (serializableObject != null)
                {
                    request.Content = new StringContent(JsonConvert.SerializeObject(serializableObject));

                    if (!string.IsNullOrEmpty(this.ContentType))
                    {
                        request.Content.Headers.ContentType = new MediaTypeHeaderValue(this.ContentType);
                    }

                }
                try
                {
                    var response = await this.Client.HttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
                    return response;
                }
                catch (Exception ex)
                {
                    dynamic responseObject = new ExpandoObject();
                    responseObject.RequestFailedReason = $"Cannot send request due to exception: {ex.Message}";
                    responseObject.RequestMetaData = request;
                    responseObject.Exception = ex;

                    HttpResponseMessage response = new HttpResponseMessage
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(responseObject, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })),
                        StatusCode = HttpStatusCode.InternalServerError
                    };

                    return response;
                }
            }
        }
    }
}
