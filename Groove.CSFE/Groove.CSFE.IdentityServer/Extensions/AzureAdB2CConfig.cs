namespace Groove.CSFE.IdentityServer.Extensions
{
    public class AzureAdB2CConfig
    {
        public const string PolicyAuthenticationProperty = "Policy";

        public string ClientId { get; set; }
        public string Tenant { get; set; }
        public string SignUpSignInPolicyId { get; set; }
        public string SignInPolicyId { get; set; }
        public string SignUpPolicyId { get; set; }
        public string ResetPasswordPolicyId { get; set; }
        public string EditProfilePolicyId { get; set; }

        public string DefaultPolicy => SignUpSignInPolicyId;
        public string GetAuthority(string policy = null)
        {
            policy = string.IsNullOrEmpty(policy) ? DefaultPolicy : policy;
            return $"https://{Tenant}.b2clogin.com/{Tenant}.onmicrosoft.com/{policy}/v2.0";
        }
        public string CallbackPath { get; set; }
        public string SignedOutCallbackPath { get; set; }
        public string SignedOutRedirectUri { get; set; }
    }
}
