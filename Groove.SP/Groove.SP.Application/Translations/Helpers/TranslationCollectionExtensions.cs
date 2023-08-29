using Groove.SP.Application.Caching;
using Groove.SP.Application.Translations.Providers;
using Groove.SP.Application.Translations.Providers.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace Groove.SP.Application.Translations.Helpers
{
    public static class TranslationCollectionExtensions
    {
        public static IServiceCollection AddTranslationProvider(this IServiceCollection services)
        {
            services.AddInMemoryCacheService();
            services.AddScoped<ITranslationProvider, TranslationProvider>();
            return services;
        }

        /// <summary>
        /// Add Request Request Localization
        /// </summary>
        /// <param name="services"></param>
        /// <param name="languageRequestHeaderKey">Header key to recoginze which language will be used for the request. Default is "api-language"</param>
        /// <returns></returns>
        public static IServiceCollection AddRequestLocalization(this IServiceCollection services, string languageRequestHeaderKey = "api-language")
        {
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedFormattingCultures = new CultureInfo[] { new CultureInfo(TranslationUtility.ENGLISH_KEY) };
                var supportedUICultures = new CultureInfo[]
                {
                    new CultureInfo(TranslationUtility.ENGLISH_KEY),
                    new CultureInfo(TranslationUtility.CHINESE_TRADITIONAL_KEY),
                    new CultureInfo(TranslationUtility.CHINESE_SIMPLIFIED_KEY)
                };

                options.DefaultRequestCulture = new RequestCulture(TranslationUtility.ENGLISH_KEY, TranslationUtility.ENGLISH_KEY);
                options.SupportedCultures = supportedFormattingCultures;
                options.SupportedUICultures = supportedUICultures;

                options.RequestCultureProviders.Insert(0, new CustomRequestCultureProvider(async context =>
                {
                    context.Request.Headers.TryGetValue(languageRequestHeaderKey, out Microsoft.Extensions.Primitives.StringValues lang);

                    if (Microsoft.Extensions.Primitives.StringValues.IsNullOrEmpty(lang))
                    {
                        return new ProviderCultureResult(TranslationUtility.ENGLISH_KEY, TranslationUtility.ENGLISH_KEY);
                    }
                    else
                    {
                        return new ProviderCultureResult(TranslationUtility.ENGLISH_KEY, lang.ToString());
                    }
                }));
            });

            return services;
        }
    }
}
