using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Groove.Infrastructure.APIClientCore
{
    public class ResponseHandler
    {      

        public async Task<T> HandleResponse<T>(HttpResponseMessage response) where T : ResponseResultBase
        {
            if (response.Content != null)
            {
                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<T>(responseString);
            }
            return default(T);
        }

        /// <summary>
        /// It is to parse response silently.
        /// In case there is exception, it will be wrapped into response
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hrm"></param>
        /// <returns></returns>
        public async Task<T> HandleResponseSilent<T>(HttpResponseMessage hrm) where T: ResponseResultBase
        {
            string responseToString = string.Empty;
            if (hrm.Content != null)
            {
                try
                {
                    var reponse = await GetResponseResult(hrm);
                    responseToString = JsonConvert.SerializeObject(reponse, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    
                    // try to de-serialize body content (Json format) to object, Status is from body content
                   var result = JsonConvert.DeserializeObject<T>(responseToString);

                    // in case that no status in response body, then try to get from response status
                    if (result.Status == 0)
                    {
                        result.Status = (int)hrm.StatusCode;
                    }

                    result.Content = responseToString;

                    return result;
                }
                catch
                {
                    var result = (T)Activator.CreateInstance(typeof(T));
                    result.Status = (int)HttpStatusCode.InternalServerError;
                    result.Content = responseToString;
                    return result;

                }
            }

            return default(T);
        }

        private async Task<dynamic> GetResponseResult(HttpResponseMessage hrm)
        {
            dynamic contentObject = new ExpandoObject();
            var content = await hrm.Content.ReadAsStringAsync().ConfigureAwait(false);
            var responseHeaders = hrm.Headers;
            var statusCode = hrm.StatusCode;
            Dictionary<string, string[]> headerDictionary = responseHeaders.ToDictionary(x => x.Key, x => x.Value.ToArray());
            var responseHeaderString = JsonConvert.SerializeObject(headerDictionary, settings: new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            var expConverter = new ExpandoObjectConverter();

            try
            { 
                if (!string.IsNullOrEmpty(content))
                {
                    
                    contentObject = JsonConvert.DeserializeObject<ExpandoObject>(content, expConverter);
                }

                contentObject.ResponseMetadata = new ExpandoObject();
                contentObject.ResponseMetadata.Headers = responseHeaderString;
                contentObject.ResponseMetadata.StatusCode = statusCode;
                contentObject.ResponseMetadata.ReceivedGMT = DateTime.UtcNow;

            }
            // In case, there is any exception
            catch (Exception ex)
            {
                var responseString = JsonConvert.SerializeObject(hrm, settings: new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                contentObject = JsonConvert.DeserializeObject<ExpandoObject>(responseString, expConverter);
                contentObject.ResponseMetadata = new ExpandoObject();
                contentObject.ResponseMetadata.ReceivedGMT = DateTime.UtcNow;
                // content here is not in json format
                contentObject.ResponseMetadata.Content = content?.Replace("\"", "\\\"");
                contentObject.Exception = ex;
            }

            return contentObject;
        }
    }

    public class ResponseResultBase
    {
        public int Status { get; set; }
        public string Content { get; set; }

    }
}
