using Groove.SP.Application.Authorization;
using Groove.SP.Application.IntegrationLog.Services.Interfaces;
using Groove.SP.Application.IntegrationLog.ViewModels;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Groove.SP.API.Filters
{
    public class AppResultFilter : IAsyncResultFilter
    {
        private readonly IIntegrationLogService _integrationLogService;
        public AppResultFilter(IIntegrationLogService integrationLogService)
        {
            _integrationLogService = integrationLogService;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (!context.ActionDescriptor.IsControllerAction())
            {
                return;
            }
            await HandleAndWrapResultExecution(context);
            await next();
        }

        private async Task HandleAndWrapResultExecution(ResultExecutingContext context)
        {
            bool.TryParse(context.HttpContext.User.FindFirstValue("is_import_client"), out bool isImportClient);

            // handle call http request from CSFE
            var methodInfos = context.ActionDescriptor.GetMethodInfo();
            var isImportApi = methodInfos.CustomAttributes.Any(x => x.AttributeType == typeof(AppImportDataAttribute));
            if (isImportClient && isImportApi)
            {
                var integrationStatus = IntegrationStatus.Succeed;
                var response = string.Empty;
                var remark = AppConstant.INTEGRATION_LOG_REMARK_DONE;
                if (context.Result is BadRequestObjectResult)
                {
                    integrationStatus = IntegrationStatus.Failed;
                    var badResult = (BadRequestObjectResult)context.Result;
                    remark = ((ValidationProblemDetails)badResult.Value).Title;
                }
                response = JsonConvert.SerializeObject(context.Result, Formatting.Indented);

                using (var reader = new StreamReader(context.HttpContext.Request.Body))
                {
                    if(context.HttpContext.Request.Body.CanSeek)
                    {
                        context.HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);
                    }
                    var body = reader.ReadToEnd();

                    var apiName = "[" + context.HttpContext.Request.Method + "] "
                                    + context.HttpContext.Request.Path.ToString().Trim('/');

                    var profile = context.HttpContext.Request.Query["profile"];
                    if(string.IsNullOrEmpty(profile))
                    {
                        profile = AppConstant.DEFAULT_PROFILE_IMPORT;
                    }
                    var viewModel = new IntegrationLogViewModel()
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
                        Response = response
                    };
                    await _integrationLogService.CreateAsync(viewModel);
                }
            }
        }

    }
}
