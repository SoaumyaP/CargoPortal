using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Groove.CSFE.IdentityServer.Extensions
{
    public static class AzureAdB2CAuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddAzureAdB2C(this AuthenticationBuilder builder)
            => builder.AddAzureAdB2C(new AzureAdB2CConfig());

        public static AuthenticationBuilder AddAzureAdB2C(this AuthenticationBuilder builder, AzureAdB2CConfig b2CConfig)
        {
            builder.AddOpenIdConnect("B2C", options =>
            {
                options.ClientId = b2CConfig.ClientId;
                options.Authority = b2CConfig.GetAuthority();
                // Set it to false to prevent issue of auto log out after a while even though renew token fine
                options.UseTokenLifetime = false;
                options.CallbackPath = b2CConfig.CallbackPath;
                options.SignedOutCallbackPath = b2CConfig.SignedOutCallbackPath;
                options.TokenValidationParameters = new TokenValidationParameters() { NameClaimType = "name" };
                options.SignedOutRedirectUri = b2CConfig.SignedOutRedirectUri;
                options.Events = new OpenIdConnectEvents()
                {
                    OnRemoteFailure = OnRemoteFailure,
                    OnRedirectToIdentityProvider = OnRedirectToIdentityProvider
                };

            }).AddOpenIdConnect("B2C-RSPW", options =>
            {
                options.ClientId = b2CConfig.ClientId;
                options.Authority = b2CConfig.GetAuthority(b2CConfig.ResetPasswordPolicyId);
                options.CallbackPath = b2CConfig.CallbackPath;
                options.SignedOutCallbackPath = b2CConfig.SignedOutCallbackPath;
            });

            return builder;
        }

        public static Task OnRemoteFailure(RemoteFailureContext context)
        {
            context.HandleResponse();
            var signedOutRedirectUri = (context.Options as OpenIdConnectOptions).SignedOutRedirectUri;
            // Handle the error code that Azure AD B2C throws when trying to reset a password from the login page 
            // because password reset is not supported by a "sign-up or sign-in policy"
            if (context.Failure is OpenIdConnectProtocolException && context.Failure.Message.Contains("AADB2C90118"))
            {
                // If the user clicked the reset password link, redirect to the reset password route
                context.Response.Redirect("/Account/ResetPassword");
            }
            else if (context.Failure is OpenIdConnectProtocolException && context.Failure.Message.Contains("access_denied"))
            {
                context.Response.Redirect(signedOutRedirectUri);
            }
            else if (context.Failure is OpenIdConnectProtocolException && context.Failure.Message.Contains("AADB2C90091"))
            {
                context.Response.Redirect(signedOutRedirectUri);
            }
            else
            {
                context.Response.Redirect(signedOutRedirectUri);
            }
            return Task.FromResult(0);
        }

        public static Task OnRedirectToIdentityProvider(RedirectContext context)
        {
            var query = context.HttpContext.Request.Query;
            //default key = 'culture' of QueryStringRequestCultureProvider
            if (query.TryGetValue("ReturnUrl", out StringValues returnUrl))
            {
                var existsCulture = QueryHelpers.ParseQuery(returnUrl).TryGetValue("culture", out StringValues culture);
                if (existsCulture)
                {
                    context.ProtocolMessage.SetParameter("ui_locales", culture);
                }
            }
            return Task.CompletedTask;
        }
    }
}
