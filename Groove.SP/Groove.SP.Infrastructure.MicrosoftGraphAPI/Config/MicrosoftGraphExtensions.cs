using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Groove.SP.Infrastructure.MicrosoftGraphAPI.Config
{
    public static class MicrosoftGraphExtensions
    {
        public static IServiceCollection AddMicrosoftGraph(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MicrosoftGraphSettings>(configuration.GetSection("MicrosoftGraphSettings"));
            services.AddScoped<IB2CUserService, B2CUserService>();

            return services;
        }
    }
}
