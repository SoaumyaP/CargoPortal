using Groove.SP.API.Filters.Authorization.Handlers;
using Groove.SP.Application.Scheduling.Services.Interfaces;
using Groove.SP.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Groove.SP.API.Middlewares
{
    public class WebhookRequestMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public WebhookRequestMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context, IServiceProvider provider)
        {
            var path = context.Request.Path;

            if (path.HasValue && path.Value.StartsWith("/webhooks", true, System.Globalization.CultureInfo.InvariantCulture))
            {
                var reportConfig = _configuration.GetSection("App:Report").Get<ReportConfig>();

                var matched = WebhookAuthenticationHelper.VerifySignature(reportConfig.WebhookSecret, context.Request, out string body);

                if (matched)
                {
                    if (path.Value.EndsWith("/scheduled", true, System.Globalization.CultureInfo.InvariantCulture))
                    {
                        if (!string.IsNullOrWhiteSpace(body))
                        {
                            dynamic bodyContent = Newtonsoft.Json.JsonConvert.DeserializeObject(body);
                            var notification = bodyContent["Notifications"][0];
                            string documentId = notification["DocumentId"];
                            string scheduleId = notification["Id"];

                            try
                            {
                                var _schedulingService = provider.GetService<ISchedulingService>();

                                await _schedulingService.UploadFtpAsync(scheduleId, documentId);
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                    }
                }

                context.Response.StatusCode = 200;
            }
            else
            {
                await _next(context);
            }
        }
    }
}
