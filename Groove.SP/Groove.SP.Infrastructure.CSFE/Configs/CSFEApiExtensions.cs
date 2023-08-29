using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Groove.SP.Infrastructure.CSFE.Configs
{
    public static class CSFEApiExtensions
    {
        public static IServiceCollection AddCSFEApi(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CSFEApiSettings>(configuration.GetSection("CSFEApiSettings"));
            services.AddScoped<ICSFEApiClient, CSFEApiClient>();

            return services;
        }
    }
}
