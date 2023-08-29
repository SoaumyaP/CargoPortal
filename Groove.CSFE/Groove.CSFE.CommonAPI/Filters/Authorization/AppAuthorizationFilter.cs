using Groove.CSFE.Application.Authorization;
using Groove.CSFE.Application.Exceptions;
using Groove.CSFE.Application.Utilities;
using Groove.CSFE.Core.Models;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace Groove.CSFE.CommonAPI.Filters.Authorization
{
    public class AppAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly List<Client> _authorizationClients;

        public AppAuthorizationFilter(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IOptions<List<Client>> options)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _authorizationClients = options.Value;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context.Filters.Any(item => item is IAllowAnonymousFilter))
            {
                return;
            }

            Boolean.TryParse(context.HttpContext.User.FindFirstValue("is_import_client"), out bool isImportClient);
            if (isImportClient)
            {
                return;
            }

            if (!context.ActionDescriptor.IsControllerAction())
            {
                return;
            }
            try
            {
                await CheckPermissions(context,
                    context.ActionDescriptor.GetMethodInfo(),
                    context.ActionDescriptor.GetMethodInfo().DeclaringType
                );
            }
            catch (AppAuthorizationException ex)
            {
                context.Result = new ObjectResult(new ErrorInfo
                {
                    Status = (int)HttpStatusCode.Unauthorized,
                    Title = ex.Message,
                    Message = ex.Message,
                    Errors = ex?.InnerException?.Message
                })
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized
                };
            }
            catch (Exception ex)
            {
                context.Result = new ObjectResult(ex.Message)
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
        }

        private async Task CheckPermissions(AuthorizationFilterContext context, MethodInfo methodInfo, Type type)
        {
            if (AllowAnonymous(methodInfo, type))
            {
                return;
            }

            var authorizeAttributes =
                ReflectionHelper
                    .GetAttributesOfMemberAndType(methodInfo, type)
                    .OfType<AppAuthorizeAttribute>()
                    .ToArray();

            if (!authorizeAttributes.Any())
            {
                return;
            }

            if (!_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                throw new AppAuthorizationException("User Not Login");
            }

            foreach (var authorizeAttribute in authorizeAttributes)
            {
                await AuthorizeAsync(context, authorizeAttribute.RequireAllPermissions, authorizeAttribute.Permissions);
            }
        }

        //helper
        private bool AllowAnonymous(MemberInfo memberInfo, Type type)
        {
            return ReflectionHelper
                .GetAttributesOfMemberAndType(memberInfo, type)
                .OfType<AppAllowAnonymousAttribute>()
                .Any();
        }

        private async Task AuthorizeAsync(AuthorizationFilterContext context, bool requireAll, params string[] permissionNames)
        {
            if (permissionNames == null || permissionNames.Count() <= 0)
            {
                return;
            }

            if (context.HttpContext.Request.Headers.TryGetValue("Authorization", out var token))
            {
                var httpClient = _httpClientFactory.CreateClient();
                var clientIdClaim = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.ClientId);
                var authorizationClient = _authorizationClients.SingleOrDefault(s => s.ClientId == clientIdClaim.Value);
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, authorizationClient.AuthorizationEndpoint);
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.ToString().Split(' ')[1]);
                httpRequestMessage.Headers.TryAddWithoutValidation("AllowPermissions", string.Join(",", permissionNames));
                httpRequestMessage.Headers.TryAddWithoutValidation("RequiredAllPermissions", requireAll.ToString());
                var result = await httpClient.SendAsync(httpRequestMessage);
                if (result.IsSuccessStatusCode && result.StatusCode == HttpStatusCode.OK)
                {
                    return;
                }
                else
                {
                    var error = await JsonSerializer.DeserializeAsync<ErrorInfo>(await result.Content.ReadAsStreamAsync());
                    throw new AppAuthorizationException(error.Message);
                }
            }
        }
    }
}
