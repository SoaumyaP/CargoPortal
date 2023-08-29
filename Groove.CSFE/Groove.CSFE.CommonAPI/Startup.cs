using AutoMapper;
using Groove.CSFE.Application.Countries.Services;
using Groove.CSFE.Application.Countries.Services.Interfaces;
using Groove.CSFE.Application.Interfaces.Repositories;
using Groove.CSFE.Application.Interfaces.UnitOfWork;
using Groove.CSFE.Application.Locations.Services;
using Groove.CSFE.Application.Locations.Services.Interfaces;
using Groove.CSFE.Application.OrganizationRoles.Services;
using Groove.CSFE.Application.OrganizationRoles.Services.Interfaces;
using Groove.CSFE.Application.Organizations.Services;
using Groove.CSFE.Application.Organizations.Services.Interfaces;
using Groove.CSFE.CommonAPI.Extensions;
using Groove.CSFE.CommonAPI.Filters.Authorization;
using Groove.CSFE.CommonAPI.Filters;
using Groove.CSFE.CommonAPI.Models;
using Groove.CSFE.Core;
using Groove.CSFE.Core.Entities;
using Groove.CSFE.Persistence.Contexts;
using Groove.CSFE.Persistence.Repositories;
using Groove.CSFE.Persistence.UnitOfWork;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RazorLight;
using RazorLight.Extensions;
using System.Collections.Generic;
using System.IO;
using Groove.CSFE.Application.AlternativeLocations.Services;
using Groove.CSFE.Application.AlternativeLocations.Services.Interfaces;
using Newtonsoft.Json;
using Groove.CSFE.Application.Carriers.Services;
using Groove.CSFE.Application.Ports.Services;
using Groove.CSFE.Persistence.Repositories.Base;
using Groove.CSFE.Application.Currencies.Services;
using Groove.CSFE.Application.EventCodes.Services;
using Groove.CSFE.Application.EventCodes.Services.Interfaces;
using Groove.CSFE.Core.Data;
using Groove.CSFE.Persistence.Data;
using Client = Groove.CSFE.CommonAPI.Filters.Authorization.Client;
using Groove.CSFE.Application.SendMails.Services.Interfaces;
using Groove.CSFE.Application.SendMails.Services;
using Groove.CSFE.Application.Vessels.Services.Interfaces;
using Groove.CSFE.Application.Vessels.Services;
using Groove.CSFE.Application.WarehouseLocations.Services.Interfaces;
using Groove.CSFE.Application.WarehouseLocations.Services;
using Groove.CSFE.Application.WarehouseAssignments.Services.Interfaces;
using Groove.CSFE.Application.WarehouseAssignments.Services;
using FluentValidation;
using Groove.CSFE.Application.Vessels.ViewModels;
using Groove.CSFE.Application.Vessels.Validations;
using Groove.CSFE.Application.EmailNotification.Services.Interfaces;
using Groove.CSFE.Application.EmailNotification.Services;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Groove.CSFE.Application.Terminals.Services.Interfaces;
using Groove.CSFE.Application.Terminals.Services;
using Groove.CSFE.Application.Warehouses.Services.Interfaces;
using Groove.CSFE.Application.Warehouses.Services;
using Groove.CSFE.Application.UserOffices.Services.Interfaces;
using Groove.CSFE.Application.UserOffices.Services;
using Groove.CSFE.Application.EventCodes.ViewModels;
using Groove.CSFE.Application.EventCodes.Validations;
using Groove.CSFE.Application.EventTypes.Services.Interfaces;
using Groove.CSFE.Application.EventTypes.Services;

namespace Groove.CSFE.CommonAPI
{
    public class Startup
    {
        private readonly string _appConnectionString;

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
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<List<Client>>(options => Configuration.GetSection("Application:Clients").Bind(options));

            // DB Contexts
            // Force EF to get each navigation property via query as same as EF 2.2
            services.AddDbContext<CsfeContext>(options => options.UseSqlServer(_appConnectionString,
                o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

            // DI
            RegisterServices(services);

            // Register the Swagger services
            services.AddSwaggerDocument();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddHttpClient();

            services.AddLocalization();

            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;

                options.Filters.Add(typeof(AppAuthorizationFilter));
                options.Filters.Add(typeof(AppExceptionFilter));
                options.Filters.Add(typeof(AppResultFilter));
            })
            // As part of ASP.NET Core 3.0, the team moved away from including Json.NET by default.
            // In order to reconfigure your ASP.NET Core 3.0 project with Json.NET, you will need to add a NuGet reference to Microsoft.AspNetCore.Mvc.NewtonsoftJson
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            })
            .AddViewLocalization();

            var authConfig = Configuration.GetSection("Authentication").Get<AuthenticationConfig>();

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

            // Razor Light
            services.AddRazorLight(() => new RazorLightEngineBuilder()
                .UseEmbeddedResourcesProject(typeof(Program))
                .UseFileSystemProject(Path.Combine(Environment.ContentRootPath, "EmailTemplates"))
                .Build());

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ImportClientOnly", policy => policy.RequireClaim("is_import_client", "True"));
            }); 

            services.Configure<AppConfig>(Configuration.GetSection("Application"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // CORS
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseHttpsRedirection();

            // Register the Swagger generator and the Swagger UI middlewares
            app.UseOpenApi();
            app.UseSwaggerUi3();

            //Localization
            app.UseAppRequestLocalization();

            // Authentication
            app.UseAuthentication();
            app.UseAuthorization();

            // CORS
            app.UseCors("AllowAll");

            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "default", template: "{controller=Home}/{action=Index}/{id?}");
                routes.MapRoute(name: "api", template: "api/{controller}/{action?}/{id?}");
            });
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

        private void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IUnitOfWorkProvider, UnitOfWorkProvider>();

            // Repository
            services.AddScoped<IRepository<CountryModel>, CountryRepository>();
            services.AddScoped<ICountryRepository, CountryRepository>();
            services.AddScoped<IRepository<LocationModel>, LocationRepository>();
            services.AddScoped<IRepository<CustomerRelationshipModel>, CustomerRelationshipRepository>();
            services.AddScoped<IRepository<OrganizationModel>, OrganizationRepository>();
            services.AddScoped<IRepository<OrganizationRoleModel>, OrganizationRoleRepository>();
            services.AddScoped<IRepository<OrganizationInRoleModel>, OrganizationInRoleRepository>();
            services.AddScoped<IRepository<CarrierModel>, CarrierRepository> ();
            services.AddScoped<IRepository<PortModel>, PortRepository>();
            services.AddScoped<IRepository<CurrencyModel>, CurrencyRepository>();
            services.AddScoped<IRepository<AlternativeLocationModel>, AlternativeLocationRepository>();
            services.AddScoped<IRepository<EventCodeModel>, EventCodeRepository>();
            services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            services.AddScoped<IRepository<VesselModel>, VesselRepository>();
            services.AddScoped<IRepository<EventTypeModel>, EventTypeRepository>();
            services.AddScoped<IRepository<WarehouseLocationModel>, WarehouseLocationRepository>();
            services.AddScoped<IRepository<WarehouseAssignmentModel>, WarehouseAssignmentRepository>();
            services.AddScoped<IRepository<EmailNotificationModel>, EmailNotificationRepository>();
            services.AddScoped<IRepository<WarehouseModel>, WarehouseRepository>();
            services.AddScoped<IRepository<TerminalModel>, TerminalRepository>();
            services.AddScoped<IRepository<UserOfficeModel>, UserOfficeRepository>();

            services.AddScoped<IOrganizationListService, OrganizationListService>();
            services.AddScoped<IDataQuery, EfDataQuery>();

            // Service
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<ILocationService, LocationService>();
            services.AddScoped<IOrganizationService, OrganizationService>();
            services.AddScoped<IOrganizationRoleService, OrganizationRoleService>();
            services.AddScoped<ISendMailService, SendMailService>();
            services.AddScoped<ICarrierService, CarrierService>();
            services.AddScoped<IPortService, PortService>();
            services.AddScoped<ICurrencyService, CurrencyService>();
            services.AddScoped<IAlternativeLocationService, AlternativeLocationService>();
            services.AddScoped<IEventCodeService, EventCodeService>();
            services.AddScoped<IEventTypeService, EventTypeService>();
            services.AddScoped<ICustomerRelationshipService, CustomerRelationshipService>();
            services.AddScoped<IVesselService, VesselService>();
            services.AddScoped<IWarehouseLocationService, WarehouseLocationService>();
            services.AddScoped<IWarehouseAssignmentService, WarehouseAssignmentService>();
            services.AddScoped<IWarehouseLocationListService, WarehouseLocationListService>();
            services.AddScoped<IEmailNotificationService, EmailNotificationService>();
            services.AddScoped<ITerminalService, TerminalService>();
            services.AddScoped<IWarehouseService, WarehouseService>();
            services.AddScoped<IUserOfficeService, UserOfficeService>();
            services.AddScoped<IEventCodeListService, EventCodeListService>();

            // Validator
            services.AddScoped<IValidator<CreateVesselViewModel>, CreateVesselViewModelValidator>();
            services.AddScoped<IValidator<UpdateVesselViewModel>, UpdateVesselViewModelValidator>();
            services.AddScoped<IValidator<UpdateEventSequenceViewModel>, UpdateEventSequenceViewModelValidator>();
        }
    }
}
