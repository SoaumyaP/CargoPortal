using Groove.SP.API.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.API.Middlewares
{
    /// <summary>
    /// Middleware to filter for only read request (via method GET) to access in userrole switch mode
    /// <br/>
    /// Other method (POST, PUT, DELETE, PATCH) will be forbidden.
    /// </summary>
    /// <remarks>
    /// <b>The middleware must be registered after UseAuthentication() and UseAuthorization() middlewares
    /// </b>
    /// </remarks>
    public class UserRoleSwitchMiddleware
    {
        private readonly RequestDelegate _next;

        public UserRoleSwitchMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
           
            var path = context.Request.Path;           
            var isUserRoleSwitch = bool.TrueString.Equals(context.User.FindFirst(AppConstants.SECURITY_USER_ROLE_SWITCH)?.Value, StringComparison.InvariantCultureIgnoreCase);
            var requestMethod = context.Request.Method?.ToLowerInvariant();
            var checkingMethods = new[]
            {
                "post", "put", "delete", "patch"
            };

            var ignoreUrls = new[]
            {
                "/api/Users/HasPermissions"
            };

            if (!ignoreUrls.Contains(path.Value) && isUserRoleSwitch && checkingMethods.Contains(requestMethod))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
            }
            else
            {
                await _next(context);
            }
        }
    }
}
