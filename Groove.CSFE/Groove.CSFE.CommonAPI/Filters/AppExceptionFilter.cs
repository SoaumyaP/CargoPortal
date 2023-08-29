using Groove.CSFE.Application.Authorization;
using Groove.CSFE.Application.Exceptions;
using Groove.CSFE.Application.IntegrationLog.ViewModels;
using Groove.CSFE.Application.Utilities;
using Groove.CSFE.CommonAPI.Filters.Authorization;
using Groove.CSFE.Core;
using Groove.CSFE.Core.Models;
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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Client = Groove.CSFE.CommonAPI.Filters.Authorization.Client;

namespace Groove.CSFE.CommonAPI.Filters
{
    public class AppExceptionFilter : IAsyncExceptionFilter
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly List<Client> _clients;

        public AppExceptionFilter(IHttpClientFactory httpClientFactory, IOptions<List<Client>> options)
        {
            _httpClientFactory = httpClientFactory;
            _clients = options.Value;
        }
        public async Task OnExceptionAsync(ExceptionContext context)
        {
            if (!context.ActionDescriptor.IsControllerAction())
            {
                return;
            }           

            await HandleAndWrapException(context);
        }

        private async Task HandleAndWrapException(ExceptionContext context)
        {
            context.HttpContext.Response.StatusCode = GetStatusCode(context);

            context.Result = new ObjectResult(new ErrorInfo
            {
                Status = GetStatusCode(context),
                Title = context.Exception.Message,
                Message = context.Exception.Message,
                Errors = context.Exception?.InnerException?.Message
            });

            bool.TryParse(context.HttpContext.User.FindFirstValue("is_import_client"), out bool isImportClient);
            if (isImportClient)
            {
                // Need to check if [AppImportData] then write integration log
                var methodInfos = context.ActionDescriptor.GetMethodInfo();
                var isImportApi = methodInfos.CustomAttributes.Any(x => x.AttributeType == typeof(AppImportDataAttribute));
                if (isImportApi)
                {
                    await SendIntegrationLog(context);
                }
            }
        }

        private async Task SendIntegrationLog(ExceptionContext context)
        {
            var clientIdClaim = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.ClientId);
            var importDataClient = _clients.SingleOrDefault(c => c.ClientId == clientIdClaim.Value);
            using (var reader = new StreamReader(context.HttpContext.Request.Body))
            {
                if (context.HttpContext.Request.Body.CanSeek)
                {
                    context.HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);
                }
                var body = reader.ReadToEnd();

                var apiName = "[" + context.HttpContext.Request.Method + "] "
                + context.HttpContext.Request.Path.ToString().Trim('/');

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
                    Status = IntegrationStatus.Failed,
                    Remark = AppException.GetTrueExceptionMessage(context.Exception),
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

        protected virtual int GetStatusCode(ExceptionContext context)
        {
            if (context.Exception is AppAuthorizationException)
            {
                return context.HttpContext.User.Identity.IsAuthenticated
                    ? (int)HttpStatusCode.Forbidden
                    : (int)HttpStatusCode.Unauthorized;
            }

            if (context.Exception is AppValidationException)
            {
                return (int)HttpStatusCode.BadRequest;
            }

            if (context.Exception is FluentValidation.ValidationException)
            {
                return (int)HttpStatusCode.BadRequest;
            }

            if (context.Exception is AppEntityNotFoundException)
            {
                return (int)HttpStatusCode.NotFound;
            }

            return (int)HttpStatusCode.InternalServerError;
        }
    }
}
