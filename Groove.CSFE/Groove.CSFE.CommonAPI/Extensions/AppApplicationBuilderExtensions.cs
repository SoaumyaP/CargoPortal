using Groove.CSFE.Application.Localization;
using Groove.CSFE.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.CSFE.CommonAPI.Extensions
{
    public static class AppApplicationBuilderExtensions
    {
        public static void UseAppRequestLocalization(this IApplicationBuilder app, Action<RequestLocalizationOptions> optionsAction = null)
        {
            var supportedCultures = new List<CultureInfo>
            {
                new CultureInfo(AppConstants.ENGLISH_KEY),
                new CultureInfo(AppConstants.CHINESE_TRADITIONAL_KEY),
                new CultureInfo(AppConstants.CHINESE_SIMPLIFIED_KEY),
            };


            var options = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(AppConstants.ENGLISH_KEY),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            };

            optionsAction?.Invoke(options);

            //0: QueryStringRequestCultureProvider

            //1: LocalizationHeaderRequestCultureProvider
            options.RequestCultureProviders.Insert(1, new AppLocalizationHeaderRequestCultureProvider());

            app.UseRequestLocalization(options);
        }
    }
}
