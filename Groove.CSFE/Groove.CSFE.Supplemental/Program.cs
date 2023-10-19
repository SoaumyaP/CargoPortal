using Groove.CSFE.Supplemental.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Tokens;
using Serilog;

Log.Logger = new LoggerConfiguration()
                .Enrich.WithProperty("CSP", "Supplemental")
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .WriteTo.File("Logs\\Log-.txt",
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: 1000000,
                    rollOnFileSizeLimit: true,
                    shared: true)
                .CreateLogger();


var builder = WebApplication
    .CreateBuilder(args);

builder.Configuration.AddJsonFile("customconnectionstring.json", optional: true, reloadOnChange: true);
//builder.Services.AddCors();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins(builder.Configuration["AllowOrigins"].Split(";"));
            policy.WithOrigins("*")
            .SetIsOriginAllowedToAllowWildcardSubdomains()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .Build();
        });
});

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IDbConnections, DbConnections>();
builder.Services.AddScoped<IBalanceOfGoodsService, BalanceOfGoodsService>();

builder.Services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.RequireHttpsMetadata = false;

                    options.Authority = builder.Configuration["Authentication:Authority"];
                    options.Audience = builder.Configuration["Authentication:Audience"];

                    options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = true
                    };

                    // if token does not contain a dot, it is a reference token
                    options.ForwardDefaultSelector = ForwardReferenceToken("introspection");
                })
                // reference tokens
                .AddOAuth2Introspection("introspection", options =>
                {
                    options.Authority = builder.Configuration["Authentication:Authority"];

                    options.ClientId = builder.Configuration["Authentication:ClientId"];
                    options.ClientSecret = builder.Configuration["Authentication:ClientSecret"];
                });

Func<HttpContext, string?>? ForwardReferenceToken(string introspectionScheme = "introspection")
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

(string, string) GetSchemeAndCredential(HttpContext context)
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
builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    //app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(option =>
            option.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                );
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
