using Groove.CSFE.Application.Authorization;
using Groove.CSFE.Application.IntegrationLog.ViewModels;
using Groove.CSFE.Application.Utilities;
using Groove.CSFE.Core;
using IdentityModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Groove.CSFE.CommonAPI.Filters.Authorization
{
    public class AppResultFilter : IAsyncResultFilter
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly List<Client> _clients;

        public AppResultFilter(IHttpClientFactory httpClientFactory, IOptions<List<Client>> options)
        {
            _httpClientFactory = httpClientFactory;
            _clients = options.Value;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            bool.TryParse(context.HttpContext.User.FindFirstValue("is_import_client"), out bool isImportClient);
            if (isImportClient)
            {
                // Need to check if [AppImportData] then write integration log
                var methodInfos = context.ActionDescriptor.GetMethodInfo();
                var isImportApi = methodInfos.CustomAttributes.Any(x => x.AttributeType == typeof(AppImportDataAttribute));
                if (isImportApi)
                {
                    await SendIntegrationLog(context, next);
                }
            }

            await next();
        }

        private async Task SendIntegrationLog(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var clientIdClaim = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.ClientId);
            var importDataClient = _clients.SingleOrDefault(c => c.ClientId == clientIdClaim.Value);

            if (importDataClient == null)
            {
                //TODO: Need logging
                return;
            }

            using (var reader = new StreamReader(context.HttpContext.Request.Body))
            {
                if (context.HttpContext.Request.Body.CanSeek)
                {
                    context.HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);
                }
                var body = reader.ReadToEnd();

                var integrationStatus = IntegrationStatus.Succeed;
                var response = JsonConvert.SerializeObject(context.Result, Formatting.Indented);
                var remark = AppConstants.INTEGRATION_LOG_REMARK_DONE;
                if (context.Result is BadRequestObjectResult)
                {
                    integrationStatus = IntegrationStatus.Failed;
                    var badResult = (BadRequestObjectResult)context.Result;
                    remark = ((ValidationProblemDetails)badResult.Value).Title;
                }
                
                var apiName = "[" + context.HttpContext.Request.Method + "] " + context.HttpContext.Request.Path.ToString().Trim('/');

                var profile = context.HttpContext.Request.Query["profile"];
                if (string.IsNullOrEmpty(profile))
                {
                    profile = AppConstants.DEFAULT_PROFILE_IMPORT;
                }

                var integrationLog = new IntegrationLogViewModel
                {
                    APIName = apiName,
                    APIMessage = "{\n \"name\" : \"" + apiName + context.HttpContext.Request.QueryString + "\", \n" +
                                        " \"body\" : \"" + body + "\" \n}",
                    EDIMessageRef = string.Empty,
                    EDIMessageType = string.Empty,
                    PostingDate = DateTime.UtcNow,
                    Profile = profile,
                    Status = integrationStatus,
                    Remark = remark,
                    Response = JsonConvert.SerializeObject(context.Result, Formatting.Indented)
                };

                var httpClient = _httpClientFactory.CreateClient();

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, importDataClient.IntegrationLogEndpoint)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(integrationLog), Encoding.UTF8, "application/json")
                };

                if (context.HttpContext.Request.Headers.TryGetValue(HeaderNames.Authorization, out var authHeader))
                {
                    httpRequestMessage.Headers.Authorization = AuthenticationHeaderValue.Parse(authHeader);
                }

                await httpClient.SendAsync(httpRequestMessage);
            }
        }
    }
}
