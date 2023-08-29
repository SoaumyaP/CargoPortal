using Microsoft.AspNetCore.Authorization;

namespace Groove.SP.API.Filters.Authorization.Requirements
{
    public class WebhookSignatureVerificationRequirement : IAuthorizationRequirement
    {
        public WebhookSignatureVerificationRequirement(string secret) =>
            Secret = secret;

        public string Secret { get; }
    }
}
