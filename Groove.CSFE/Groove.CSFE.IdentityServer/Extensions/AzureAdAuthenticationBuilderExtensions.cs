using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Threading.Tasks;

namespace Groove.CSFE.IdentityServer.Extensions
{
    public static class AzureAdAuthenticationBuilderExtensions
    {        
        public static AuthenticationBuilder AddAzureAd(this AuthenticationBuilder builder)
            => builder.AddAzureAd(new AzureAdConfig());

        public static AuthenticationBuilder AddAzureAd(this AuthenticationBuilder builder, AzureAdConfig azureConfig)
        {
            builder.AddOpenIdConnect("AD", "Azure AD", options =>
            {
                options.ClientId = azureConfig.ClientId;
                // V2 specific
                options.Authority = azureConfig.Authority;   
                // Set it to false to prevent issue of auto log out after a while even though renew token fine
                options.UseTokenLifetime = false;
                options.RequireHttpsMetadata = false;
                // Accept several tenants
                options.TokenValidationParameters.ValidateIssuer = false;     
                options.CallbackPath = azureConfig.CallbackPath;
                options.SignedOutCallbackPath = azureConfig.SignedOutCallbackPath;
                options.SignedOutRedirectUri = azureConfig.SignedOutRedirectUri;
                options.Events = new OpenIdConnectEvents
                {
                    OnRemoteFailure = OnRemoteFailure
                };

            });
            return builder;
        }

        public static Task OnRemoteFailure(RemoteFailureContext context)
        {
            context.HandleResponse();
            var signedOutRedirectUri = (context.Options as OpenIdConnectOptions).SignedOutRedirectUri;
            context.Response.Redirect(signedOutRedirectUri);
            return Task.FromResult(0);
        }
    }
}
