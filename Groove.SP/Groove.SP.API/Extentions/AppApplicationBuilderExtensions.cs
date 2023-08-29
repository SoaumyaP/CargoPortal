using Groove.SP.API.Middlewares;
using Groove.SP.Application.Localization;
using Groove.SP.Application.Translations.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Groove.SP.API.Extentions
{
    public static class AppApplicationBuilderExtensions
    {
        public static void UseAppRequestLocalization(this IApplicationBuilder app, Action<RequestLocalizationOptions> optionsAction = null)
        {
            var supportedCultures = new List<CultureInfo>
            {
                new CultureInfo(TranslationUtility.ENGLISH_KEY),
                new CultureInfo(TranslationUtility.CHINESE_TRADITIONAL_KEY),
                new CultureInfo(TranslationUtility.CHINESE_SIMPLIFIED_KEY),
            };


            var options = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(TranslationUtility.ENGLISH_KEY),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            };

            optionsAction?.Invoke(options);

            //0: QueryStringRequestCultureProvider

            //1: LocalizationHeaderRequestCultureProvider
            options.RequestCultureProviders.Insert(1, new AppLocalizationHeaderRequestCultureProvider());

            app.UseRequestLocalization(options);
        }

        /// <summary>
        /// To register middleware that handles webhookrequest from Telerik report server.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseWebhookRequest(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<WebhookRequestMiddleware>();
        }

        /// <summary>
        /// To register middleware that handles requests in userrole switch mode.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder CheckUserRoleSwitchRequest(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UserRoleSwitchMiddleware>();

        }
    }
}
