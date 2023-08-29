using System;
using System.Net.Http;

namespace Groove.Infrastructure.APIClientCore
{
    public class BaseClient
    {
        private string baseUrl;
        private readonly IHttpClientFactory _httpClientFactory;
        public HttpClient HttpClient { get; private set; }

        public BaseClient(string baseUrl,
            IHttpClientFactory httpClientFactory)
        {
            this.BaseUrl = baseUrl;
            this._httpClientFactory = httpClientFactory;
            HttpClient = _httpClientFactory.CreateClient();
        }

        public string BaseUrl
        {
            get { return this.baseUrl; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new Exception(ErrorConstants.Messages.BaseUrlMissing);
                }

                this.baseUrl = value.TrimEnd('/');
            }
        }
    }
}
