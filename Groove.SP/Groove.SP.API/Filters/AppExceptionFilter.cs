using Groove.SP.Application.Authorization;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.IntegrationLog.Services.Interfaces;
using Groove.SP.Application.IntegrationLog.ViewModels;
using Groove.SP.Application.Translations.Providers.Interfaces;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Groove.SP.API.Filters
{
    public class AppExceptionFilter : IAsyncExceptionFilter
    {
        private readonly ITranslationProvider _translationProvider;

        private readonly IIntegrationLogService _integrationLogService;

        private readonly ILogger<AppExceptionFilter> _logger;

        public AppExceptionFilter(ITranslationProvider translationProvider,
            IIntegrationLogService integrationLogService,
            ILogger<AppExceptionFilter> logger)
        {
            _translationProvider = translationProvider;
            _integrationLogService = integrationLogService;
            _logger = logger;
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
            //if (!ActionResultHelper.IsObjectResult(context.ActionDescriptor.GetMethodInfo().ReturnType))
            //{
            //    return;
            //}

            context.HttpContext.Response.StatusCode = GetStatusCode(context);

            context.Result = new ObjectResult(new ErrorInfo
            {
                Status = GetStatusCode(context),
                Title = context.Exception.Message,
                Message = await _translationProvider.GetTranslationByKeyAsync(context.Exception.Message),
                Errors = IsEdisonBookingException(context.Exception) ? EdisonBookingErrorResult(context.Exception) : AppException.GetTrueExceptionMessage(context.Exception)
            });

            if (GetStatusCode(context) == 500)
            {
                _logger.LogError(context.Exception, context.Exception.Message);
            }

            bool.TryParse(context.HttpContext.User.FindFirstValue("is_import_client"), out bool isImportClient);

            // handle call http request from CSFE
            var methodInfos = context.ActionDescriptor.GetMethodInfo();
            var isImportApi = methodInfos.CustomAttributes.Any(x => x.AttributeType == typeof(AppImportDataAttribute));

            if (isImportClient && isImportApi)
            {
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
                        Status = IntegrationStatus.Failed,
                        Remark = AppException.GetTrueExceptionMessage(context.Exception),
                        Response = JsonConvert.SerializeObject(context.Result, Formatting.Indented)
                    };
                    await _integrationLogService.CreateAsync(viewModel);
                }
            }

            // Handle for EdisonBookingException
            // Send booking and cancel booking
            await HandleEdisonBookingExceptionAsync(context);
        }

        /// <summary>
        /// To check whether it is exception from ediSON booking service
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        private bool IsEdisonBookingException(Exception exception)
        {
            return exception.GetType() == typeof(EdisonBookingException);
        }

        /// <summary>
        /// To get result from ediSON API
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        private string EdisonBookingErrorResult(Exception exception)
        {
            var isEdisonBookingException = IsEdisonBookingException(exception);
            if (!isEdisonBookingException)
            {
                return string.Empty;
            }
            var edisonBookingException = exception as EdisonBookingException;           
            try
            {
                var edisonResult = edisonBookingException.EdisonResult;
                return edisonResult;
            }
            catch
            {
                return string.Empty;
            }
           
        } 

        /// <summary>
        /// To handle ediSON booking exception
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task HandleEdisonBookingExceptionAsync(ExceptionContext context)
        {
            var isEdisonBookingException = IsEdisonBookingException(context.Exception);
            if (!isEdisonBookingException)
            {
                return;
            }
            var exception = context.Exception as EdisonBookingException;
            IntegrationLogModel additionalData = (exception?.AdditionalData as IntegrationLogModel) ?? null;
            if (additionalData == null)
            {
                return;
            }
            var viewModel = new IntegrationLogViewModel()
            {
                APIName = additionalData.APIName,
                APIMessage = additionalData.APIMessage,
                EDIMessageRef = additionalData.EDIMessageRef,
                EDIMessageType = additionalData.EDIMessageType,
                PostingDate = additionalData.PostingDate,
                Profile = additionalData.Profile,
                Status = additionalData.Status,
                Remark = additionalData.Remark,
                Response = additionalData.Response
            };
            await _integrationLogService.CreateAsync(viewModel);
        }

        /// <summary>
        /// To get status code from exception
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
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
