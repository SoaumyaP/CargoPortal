using AutoMapper;
using Groove.SP.API.Extentions;
using Groove.SP.API.Filters.Authorization;
using Groove.SP.API.Filters;
using Groove.SP.API.Models;
using Groove.SP.Application.Translations.Helpers;
using Groove.SP.Core.Models;
using Groove.SP.Persistence.Contexts;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RazorLight;
using RazorLight.Extensions;
using System.IO;
using Groove.SP.Application.Providers.BlobStorage;
using Groove.SP.Application.Providers.Azure;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Groove.SP.Application.Utilities;
using Newtonsoft.Json;
using Groove.SP.Infrastructure.BlobStorage;
using Groove.SP.Infrastructure.Azure;
using Groove.SP.Infrastructure.CSFE.Configs;
using Groove.SP.Infrastructure.MicrosoftGraphAPI.Config;
using Groove.SP.Application.POFulfillment;
using Groove.SP.Application.BuyerApproval;
using Groove.SP.Infrastructure.Hangfire;
using MediatR;
using Groove.SP.Application.Activity.DomainEventHandlers;
using Groove.SP.Infrastructure.Excel;
using Groove.SP.Application.AppDocument.Services.Interfaces;
using Groove.SP.Application.Consolidation;
using Groove.SP.Application.ContractMaster.BackgroundJobs;
using Groove.SP.Application.Reports.BackgroundJobs;
using Microsoft.Extensions.Hosting;
using Groove.SP.Application.Translations.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using Groove.SP.API.Filters.Authorization.Requirements;
using Groove.SP.Application.Attachment.BackgroundJobs;
using Groove.SP.Application.HangfireJob;
using Groove.SP.Infrastructure.SignalR.HubConfigs;
using Groove.SP.Application.RoutingOrder.BackgroundJobs;

namespace Groove.SP.API
{
    public partial class Startup
    {
        private readonly string _appConnectionString;
        private readonly string _app2ndConnectionString;
        private readonly string _hangfireConnectionString;

        private const string AllCors = "AllCors";

        public IWebHostEnvironment Environment { get; }

        public IConfigurationRoot Configuration { get; }

        public Startup(IWebHostEnvironment environment)
        {
            var builder = new ConfigurationBuilder().SetBasePath(environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true)
                .AddJsonFile("customconnectionstring.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"customconnectionstring.{environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
            Environment = environment;
            _appConnectionString = Configuration.GetConnectionString("DefaultConnection");
            _app2ndConnectionString = Configuration.GetConnectionString("SecondaryConnection");
            _hangfireConnectionString = Configuration.GetConnectionString("HangfireConnection");
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddTranslationProvider();
            services.AddRequestLocalization();

            // App Settings
            services.Configure<AppConfig>(Configuration.GetSection("App"));
            services.Configure<EBookingManagementAPIConfig>(Configuration.GetSection("EBookingManagementAPI"));
            services.Configure<TelemetryConfig>(Configuration.GetSection("Telemetry"));
            services.Configure<CSEDShippingDocumentServiceBus>(Configuration.GetSection("CSEDShippingDocumentServiceBus"));
            services.Configure<CSEDShippingDocumentCredential>(Configuration.GetSection("CSEDShippingDocumentCredential"));
            services.Configure<SFTPRoutingOrderServerProfile>(Configuration.GetSection("SFTPRoutingOrderServerProfile"));
            services.Configure<CustomerOrgReference>(Configuration.GetSection("CustomerOrgReference"));

            // Application data connection configs
            services.AddSingleton(new AppDbConnections
            {
                CsPortalDb = _appConnectionString,
                SecondaryCsPortalDb = _app2ndConnectionString,
                HangfireDb = _hangfireConnectionString
            });

            var allowedOrigins = Configuration.GetSection("CORS:Origins").Value.Split(',');
            services.AddCors(options =>
            {
                options.AddPolicy(AllCors,
                    builder =>
                    {
                        builder.WithOrigins(allowedOrigins)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    });
            });

            // DB Contexts
            // Force EF to get each navigation property via query as same as EF 2.2
            services.AddDbContext<SpContext>(options => options.UseSqlServer(_appConnectionString,
                o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));


            // Hangfire
            if (string.IsNullOrEmpty(_hangfireConnectionString))
            {
                // Use memory storage for pre-production, no job would be create/executed
                services.AddHangfire(config => config.UseInMemoryStorage());
            }
            else
            {
                // User SQL database storage for other environment
                // Increase comment timeout as storing file content to db prior to send email
                var hangfireOptions = new Hangfire.SqlServer.SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromSeconds(300),
                    CommandTimeout = TimeSpan.FromSeconds(300)
                };
                services.AddHangfire(config => config.UseSqlServerStorage(_hangfireConnectionString, hangfireOptions));
            }
            if (int.TryParse(Configuration["Hangfire:JobRetentionTimeInDay"], out var retentionTimeInDay))
            {
                GlobalJobFilters.Filters.Add(new LongTimeRetentionJobAttribute(retentionTimeInDay));
            }
            else
            {
                // Default retention time for successful/deleted jobs is in 1 day
                GlobalJobFilters.Filters.Add(new LongTimeRetentionJobAttribute(1));
            }

            services.AddHangfireServer();

            // DI
            RegisterServices(services);

            // Register the Swagger services
            services.AddSwaggerDocument();

            // Data in-memory cache
            services.AddMemoryCache();


            // AutoMapper
            services.AddAutoMapper(cfg => cfg.AddProfile<TranslationsMappingProfile>(), AppDomain.CurrentDomain.GetAssemblies());
            services.AddLocalization();
            services.AddMvc(options =>
                {
                    options.EnableEndpointRouting = false;

                    options.Filters.Add(typeof(AppAuthorizationFilter));
                    options.Filters.Add(typeof(AppExceptionFilter));
                    options.Filters.Add(typeof(AppResultFilter));
                    options.Filters.Add(typeof(CultureFilter));
                    options.Filters.Add(typeof(QueuedBackgroundJobsExecutionFilter));
                    options.Filters.Add(typeof(AppDataMemoryCacheFilter));
                })
                .AddViewLocalization();


            var authConfig = Configuration.GetSection("Authentication").Get<AuthenticationConfig>();
            var reportConfig = Configuration.GetSection("App:Report").Get<ReportConfig>();

            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.RequireHttpsMetadata = false;

                    options.Authority = authConfig.Authority;

                    options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false
                    };

                    // if token does not contain a dot, it is a reference token
                    options.ForwardDefaultSelector = ForwardReferenceToken("introspection");
                })
                // reference tokens
                .AddOAuth2Introspection("introspection", options =>
                {
                    options.Authority = authConfig.Authority;

                    options.ClientId = authConfig.ApiName;
                    options.ClientSecret = authConfig.ApiSecret;
                });

            services.AddMvcCore()
                .AddAuthorization(options =>
                {
                    options.AddPolicy("ImportClientOnly", policy => policy.RequireClaim("is_import_client", "True"));
                    options.AddPolicy("WebhookSignatureVerification", policy => policy.Requirements.Add(new WebhookSignatureVerificationRequirement(reportConfig.WebhookSecret)));
                    options.AddPolicy(AppConstants.SECURITY_MOBILE_APP_POLICY, policy => policy.Requirements.Add(new MobileAppSecurityVerificationRequirement(Configuration.GetSection("Mobile:AppAgent").Value, Configuration.GetSection("Mobile:SecureSecret").Value)));
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });

            // Razor Light
            services.AddRazorLight(() => new RazorLightEngineBuilder()
                .UseFileSystemProject(Path.Combine(Environment.ContentRootPath, "EmailTemplates"))
                .Build());


            services.AddHttpClient();
            services.AddCSFEApi(Configuration);
            services.AddMicrosoftGraph(Configuration);
            services.AddMediatR(typeof(ContainerActivityCreatedDomainEventHandler));
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebhookRequest();
            }
            else
            {
                InitializeAzureStorage(serviceProvider).Wait();

                app.UseWebhookRequest();

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // CORS
            app.UseCors(AllCors);

            app.UseRouting();

            app.UseHttpsRedirection();

            // Register the Swagger generator and the Swagger UI middlewares
            app.UseOpenApi();
            app.UseSwaggerUi3();

            //Localization
            app.UseAppRequestLocalization();

            // Authentication
            app.UseAuthentication();
            app.UseAuthorization();

            // Register middleware to check request base on user role switch mode
            app.CheckUserRoleSwitchRequest();

            app.UseMvc(routes =>
            {

                routes.MapRoute(name: "default", template: "{controller=Home}/{action=Index}/{id?}");
                routes.MapRoute(name: "api", template: "api/{controller}/{action?}/{id?}");
            });

            app.UseEndpoints(endpoints => {
                endpoints.MapHub<NotificationHub>("hub/notification");
            });

            // Hangfire
            var hangfireDashboardUrl = Configuration["Hangfire:DashboardUrl"] ?? "/hangfire";
            app.UseHangfireDashboard(hangfireDashboardUrl, new DashboardOptions
            {
                Authorization = new[] { new HangfireDashboardAuthorizationFilter(Environment) }
            });


            if (!string.IsNullOrEmpty(_hangfireConnectionString))
            {
                StartCronJobs();
                StartIntegrationJobs(serviceProvider);
            }
        }

        private void StartCronJobs()
        {
            RecurringJob.AddOrUpdate<ResetPOFFSequenceValueJob>(j => j.ExecuteAsync(), Cron.Monthly);
            RecurringJob.AddOrUpdate<ResetPOFFLoadSequenceValueJob>(j => j.ExecuteAsync(), Cron.Monthly);
            RecurringJob.AddOrUpdate<ResetBuyerApprovalSequenceValueJob>(j => j.ExecuteAsync(), Cron.Monthly);
            RecurringJob.AddOrUpdate<ResetLoadPlanIDSequenceValueJob>(j => j.ExecuteAsync(), Cron.Monthly);
            RecurringJob.AddOrUpdate<AutoExpireContractMasterStatusJob>(j => j.ExecuteAsync(), Cron.Daily(0, 10));
            RecurringJob.AddOrUpdate<RemoveFailedJob>(j => j.Execute(), Cron.Monthly(2, 10));

            // Every 5 minutes
            RecurringJob.AddOrUpdate<ImportRoutingOrderJob>(j => j.ExecuteAsync(), "*/5 * * * *");

            // Every 20 minutes
            RecurringJob.AddOrUpdate<AutoPingTelerikReportServerStatusJob>(j => j.ExecuteAsync(), "*/20 * * * *");

            if (!Environment.IsDevelopment())
            {
                // At 01:00 on day-of-month 1
                RecurringJob.AddOrUpdate<AzureBlobsCleanupJob>(j => j.AzureAttachmentCleanupJob(), "0 1 1 * *");
                // At 02:30 on day-of - month 1
                RecurringJob.AddOrUpdate<AzureBlobsCleanupJob>(j => j.AzureSharedDocumentsCleanupJob(), "30 2 1 * *");
            }
        }

        private void StartIntegrationJobs(IServiceProvider serviceProvider)
        {
            // subscribes Azure Service Bus
            var ediSonShippingDocumentProcessor = serviceProvider.GetService<ICSEDShippingDocumentProcessor>();
            ediSonShippingDocumentProcessor.StartProccessorAsync();
        }


        private void RegisterServices(IServiceCollection services)
        {

            this.RegisterDAL(services);

            this.RegisterBusinessServices(services);

            this.RegisterProviders(services);

            this.RegisterValidators(services);

            if (Environment.IsDevelopment())
            {
                services.AddSingleton<IBlobStorage, FileSystemBlobStorage>();
                services.AddSingleton<IAzureBlobContext, AzureBlobContext>();
            }
            else
            {
                services.AddSingleton<IBlobStorage, AzureBlobStorage>();
                services.AddSingleton<IAzureBlobContext, AzureBlobContext>();
            }

            services.AddScoped<IExportManager, ExportManager>();
        }

        private async Task InitializeAzureStorage(IServiceProvider serviceProvider)
        {
            var blobContext = serviceProvider.GetService<IAzureBlobContext>();

            var tasks = new List<Task>();

            // create all blob containers
            var blobCategories = typeof(BlobCategories).GetAllPublicConstantValues<string>();
            foreach (var category in blobCategories)
            {
                var task = blobContext.CreateContainerAsync(category);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Provides a forwarding func for JWT vs reference tokens (based on existence of dot in token)
        /// </summary>
        /// <param name="introspectionScheme">Scheme name of the introspection handler</param>
        /// <returns></returns>
        public static Func<HttpContext, string> ForwardReferenceToken(string introspectionScheme = "introspection")
        {
            string Select(HttpContext context)
            {
                var (scheme, credential) = GetSchemeAndCredential(context);

                if (scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase) &&
                    !credential.Contains("."))
                {
                    return introspectionScheme;
                }

                return null;
            }

            return Select;
        }

        /// <summary>
        /// Extracts scheme and credential from Authorization header (if present)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static (string, string) GetSchemeAndCredential(HttpContext context)
        {
            var header = context.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(header))
            {
                return ("", "");
            }

            var parts = header.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                return ("", "");
            }

            return (parts[0], parts[1]);
        }
    }


}
