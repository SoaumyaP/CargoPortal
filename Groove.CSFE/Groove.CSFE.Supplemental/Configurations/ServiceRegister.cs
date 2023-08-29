using Groove.CSFE.Supplemental.Services;

namespace Groove.CSFE.Supplemental.Configurations
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddSupplementalServices(this IServiceCollection services)
        {
            services.AddScoped<IBalanceOfGoodsService, BalanceOfGoodsService>();
            services.AddScoped<IDbConnections, DbConnections>();

            return services;
        }
    }
}
