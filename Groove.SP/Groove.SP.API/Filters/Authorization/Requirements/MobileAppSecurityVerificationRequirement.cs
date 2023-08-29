using Microsoft.AspNetCore.Authorization;

namespace Groove.SP.API.Filters.Authorization.Requirements
{
    public class MobileAppSecurityVerificationRequirement : IAuthorizationRequirement
    {
        public string SecureSecret { get; }

        public string AppAgent { get; }

        public MobileAppSecurityVerificationRequirement(string appAgent, string secureSecret)
        {
            AppAgent = appAgent;
            SecureSecret = secureSecret;
        }
    }
}
