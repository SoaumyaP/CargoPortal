using Groove.SP.API.Filters.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Groove.SP.API.Filters.Authorization.Handlers
{
    public class MobileAppSecurityVerificationHandler : AuthorizationHandler<MobileAppSecurityVerificationRequirement>, IAuthorizationRequirement
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public MobileAppSecurityVerificationHandler(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MobileAppSecurityVerificationRequirement requirement)
        {
            var requestHeaders = _httpContextAccessor.HttpContext.Request.Headers;

            var appAgent = _configuration.GetSection("Mobile:AppAgent").Value;
            requestHeaders.TryGetValue("App-Agent", out var headerValue);
            if (headerValue != appAgent)
            {
                return Task.CompletedTask;
            }
            var secureSecret = _configuration.GetSection("Mobile:SecureSecret").Value;
            requestHeaders.TryGetValue("Secure-Secret", out headerValue);
            if (headerValue != secureSecret)
            {
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
