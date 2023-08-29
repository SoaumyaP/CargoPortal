using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Groove.CSFE.IdentityServer.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Groove.CSFE.IdentityServer.Models;
using Groove.CSFE.IdentityServer.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Security.Cryptography;
using System.Security.Claims;
using IdentityModel;
using Microsoft.IdentityModel.Logging;
using Microsoft.Extensions.Hosting;
using Groove.CSFE.IdentityServer.Services.Interfaces;

namespace Groove.CSFE.IdentityServer
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile("customconnectionstring.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"customconnectionstring.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IdentityModelEventSource.ShowPII = true;
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddSingleton<IConfiguration>(Configuration);

            services.Configure<AzureAdConfig>(Configuration.GetSection("IdentityProvider:AzureAd"));

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
            .AddAzureAd(Configuration.GetSection("IdentityProvider:AzureAd").Get<AzureAdConfig>())
            .AddAzureAdB2C(Configuration.GetSection("IdentityProvider:AzureAdB2C").Get<AzureAdB2CConfig>())
            .AddCookie(options =>
            {
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.Headers["Location"] = context.RedirectUri;
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            var apiScopes = new[]
            {
                new ApiScope("spapi", "SP API"),
                new ApiScope("spapi.updateactivities", "UPDATE ACTIVITIES API"),
                new ApiScope("spapi.importfreightshipment", "IMPORT FREIGHT SHIPMENT API"),
                new ApiScope("csfecommonapi", "CSFE API"),
                new ApiScope("supplementalapi", "CS PO PORTAL SUPPLEMENTAL API"),
            };

            // Identity Server
            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryPersistedGrants()
                .AddInMemoryIdentityResources(IdentityConfig.GetIdentityResources())
                .AddInMemoryApiScopes(apiScopes)
                .AddInMemoryApiResources(IdentityConfig.GetApiResources(Configuration))
                .AddInMemoryClients(IdentityConfig.GetClients(Configuration))
                .AddProfileService<IdentityWithAdditionalClaimsProfileService>();

            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(1);
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            // CORS
            services.AddCors(c => c.AddPolicy("AllowAll",
                builder => builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetIsOriginAllowed(origin => true)
                    .AllowCredentials()));

            services.AddLocalization();

            services.AddMvc(
                options => { options.EnableEndpointRouting = false; }
                )
                .AddViewLocalization()
                .AddSessionStateTempDataProvider();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            TurnOffMicrosoftJWTMapping();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            //Localization
            app.UseAppRequestLocalization();

            app.UseSession();
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always,
                Secure = CookieSecurePolicy.Always,
                MinimumSameSitePolicy = SameSiteMode.None
            });

            app.UseAuthentication();

            // Identity Server
            app.UseIdentityServer();

            // CORS
            app.UseCors("AllowAll");

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void TurnOffMicrosoftJWTMapping()
        {
            //The long claim names come from Microsoft’s JWT handler trying to map some claim types to .NET’s ClaimTypes class types. 
            //We can turn off this behavior with the following line of code (in Startup).
            //The purpose is to change the claim System.Security.Claims.ClaimTypes.NameIdentifier to IdentityModel.JwtClaimTypes.Subject
            // to match with IdentityServerAuthenticationService
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }

        #region Identity Config

        private static class IdentityConfig
        {
            private const string GRANT_TYPE_IMPLICIT = "Implicit";

            private const string GRANT_TYPE_CLIENT_CREDENTIALS = "ClientCredentials";

            public static IEnumerable<ApiResource> GetApiResources(IConfigurationRoot config)
            {
                var appConfig = config.GetSection("Application").Get<AppConfig>();

                var apis = appConfig?.APIs.Select(a =>
                    new ApiResource(a.Name, a.DisplayName)
                    {
                        ApiSecrets = a.Secrets.Select(s => new Secret(s.Sha256())).ToList(),
                        Scopes = a.Scopes
                    });

                return apis;
            }

            public static IEnumerable<Client> GetClients(IConfigurationRoot config)
            {
                var appConfig = config.GetSection("Application").Get<AppConfig>();

                var clients = appConfig?.Clients.Where(c => c.GrantType == GRANT_TYPE_IMPLICIT).Select(c =>
                    new Client()
                    {
                        ClientId = c.ClientId,
                        ClientName = c.ClientName,
                        RequireConsent = false,
                        AllowRememberConsent = true,
                        AccessTokenType = AccessTokenType.Reference,
                        AccessTokenLifetime = c.AccessTokenLifetimeInSecond,
                        IdentityTokenLifetime = c.IdentityTokenLifetimeInSecond,
                        AllowedGrantTypes = GrantTypes.Implicit,
                        AlwaysIncludeUserClaimsInIdToken = true,
                        AllowAccessTokensViaBrowser = true,
                        UpdateAccessTokenClaimsOnRefresh = true,
                        AllowOfflineAccess = true,
                        RedirectUris = c.RedirectUris,
                        PostLogoutRedirectUris = c.PostLogoutRedirectUris,
                        AllowedCorsOrigins = c.AllowedCorsOrigins,
                        AllowedScopes = c.AllowedScopes
                    }).ToList();

                clients.AddRange(appConfig?.Clients.Where(c => c.GrantType == GRANT_TYPE_CLIENT_CREDENTIALS).Select(c =>
                    new Client()
                    {
                        ClientId = c.ClientId,
                        AllowedGrantTypes = GrantTypes.ClientCredentials,
                        ClientSecrets = c.ClientSecrets.Select(s => new Secret(s.Sha256())).ToList(),
                        AllowedScopes = c.AllowedScopes,
                        AccessTokenLifetime = c.AccessTokenLifetimeInSecond,
                        ClientClaimsPrefix = string.Empty,
                        Claims = new List<ClientClaim>()
                        {
                            new ClientClaim(JwtClaimTypes.PreferredUserName, c.ClientName),
                            new ClientClaim("is_internal", "True"),
                            new ClientClaim("is_import_client", c.IsImportClient.ToString())
                        },
                        AccessTokenType = AccessTokenType.Jwt
                    }));

                return clients;
            }

            public static IEnumerable<IdentityResource> GetIdentityResources()
            {
                return new List<IdentityResource>
                {
                    new IdentityResources.OpenId(),
                    new IdentityResources.Profile(),
                    new IdentityResources.Email()
                };
            }

            public static List<TestUser> GetUsers(IWebHostEnvironment env)
            {
                if (!env.IsDevelopment())
                {
                    return new List<TestUser>();
                }

                return new List<TestUser> { new TestUser { SubjectId = "1", Username = "apitest", Password = "1234" }, };
            }
        }

        #endregion
    }
}
