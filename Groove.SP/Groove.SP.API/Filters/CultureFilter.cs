using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using System;
using System.Globalization;
using System.Linq;

namespace Groove.SP.API.Filters
{
    public class CultureFilter : IResourceFilter
    {
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            
        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            var cookieCultures = context.HttpContext.Request.Cookies["Culture"];

            if (cookieCultures == null)
                return;

            var cultures = CookieRequestCultureProvider.ParseCookieValue(cookieCultures);

            var culture = cultures.Cultures.FirstOrDefault().Value;
            var uICulture = cultures.UICultures.FirstOrDefault().Value;

            CultureInfo.CurrentCulture = new CultureInfo(culture);
            CultureInfo.CurrentUICulture = new CultureInfo(uICulture);

            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(culture);
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(uICulture);
        }
    }
}
