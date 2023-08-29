using Microsoft.Extensions.DependencyInjection;

namespace Groove.SP.Application.Caching
{
    public static class CachingServiceCollectionExtensions
    {
        public static IServiceCollection AddInMemoryCacheService(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, MemoryCacheService>();
            return services;
        }
    }
}
