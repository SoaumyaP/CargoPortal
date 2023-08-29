using Groove.SP.API.Filters.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.API.Filters.Authorization.Handlers
{
    public class WebhookSignatureVerificationHandler : AuthorizationHandler<WebhookSignatureVerificationRequirement>, IAuthorizationRequirement
    {
        protected readonly IHttpContextAccessor _httpContextAccessor;

        public WebhookSignatureVerificationHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, WebhookSignatureVerificationRequirement requirement)
        {
            var matched = WebhookAuthenticationHelper.VerifySignature(requirement.Secret, _httpContextAccessor.HttpContext.Request, out string body);
            if (matched)
            {
                context.Succeed(requirement);

                // Rewind body context
                byte[] requestData = Encoding.UTF8.GetBytes(body);
                _httpContextAccessor.HttpContext.Request.Body = new MemoryStream(requestData);
            }

            return Task.CompletedTask;
        }
    }

    public static class WebhookAuthenticationHelper
    {
        /// <summary>
        /// The signature is a hash-based messsage authentication code (HMAC) of a shared secret + the plain message.
        /// </summary>
        /// <param name="secret"></param>
        /// <param name="request"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static bool VerifySignature(string secret, Microsoft.AspNetCore.Http.HttpRequest request, out string body)
        {
            var headers = request.Headers;

            // Read request body to stream
            using (var reader = new StreamReader(request.Body))
            {
                // Read request body to string
                body = reader.ReadToEndAsync().Result;
                ASCIIEncoding encoding = new ASCIIEncoding();

                // Shared to between Telerik and API
                var keyBytes = encoding.GetBytes(secret);
                var hasher = new HMACSHA256(keyBytes);

                // Convert request body to byte array
                var bodyContent = encoding.GetBytes(body);

                // Calculate signature of request body with the shared key
                var signatureBytes = hasher.ComputeHash(bodyContent);
                string signature = $"sha256={BitConverter.ToString(signatureBytes).Replace("-", "").ToUpper()}";

                // Read value of request header ms-signature
                if (headers.TryGetValue("ms-signature", out var headerSignature))
                {
                    // Compared
                    var matched = signature == headerSignature.ToString();

                    if (matched)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
