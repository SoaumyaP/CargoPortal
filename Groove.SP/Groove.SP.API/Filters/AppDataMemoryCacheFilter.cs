using Groove.SP.Application.Utilities;
using Groove.SP.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.API.Filters
{
    public class AppDataMemoryCacheFilter : IAsyncActionFilter, IAsyncResultFilter
    {
        private readonly IMemoryCache _memoryCache;
        private readonly int _appDataMemoryCacheInSeconds;

        public AppDataMemoryCacheFilter(IMemoryCache memoryCache, IOptions<AppConfig> appConfig)
        {
            _memoryCache = memoryCache;
            _appDataMemoryCacheInSeconds = appConfig.Value?.AppDataMemoryCacheInSeconds ?? 0;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (_appDataMemoryCacheInSeconds > 0)
            {
                var methodInfo = context.ActionDescriptor.GetMethodInfo();
                var type = context.ActionDescriptor.GetMethodInfo().DeclaringType;

                var cacheMetadata = ReflectionHelper
                                    .GetAttributesOfMemberAndType(methodInfo, type)
                                    .OfType<AppDataMemoryCacheAttribute>()
                                    .FirstOrDefault();

                if (cacheMetadata != null)
                {
                    var currentUserEmail = context.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "email")?.Value ?? string.Empty;
                    var cacheName = cacheMetadata.CacheName ?? $"{currentUserEmail}|{context.HttpContext.Request.Path}{context.HttpContext.Request.QueryString}";
                    if (_memoryCache.TryGetValue(cacheName, out IActionResult cachedData))
                    {
                        context.Result = cachedData;
                        // Add custom response header to identify that data is from cache
                        context.HttpContext.Response.Headers.Add("CSPortal-Data-From-Cache", "true");
                        return;
                    }
                }
            }

            await next();
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {

            await next();

            if (_appDataMemoryCacheInSeconds > 0)
            {
                var methodInfo = context.ActionDescriptor.GetMethodInfo();
                var type = context.ActionDescriptor.GetMethodInfo().DeclaringType;

                var cacheMetadata = ReflectionHelper
                                    .GetAttributesOfMemberAndType(methodInfo, type)
                                    .OfType<AppDataMemoryCacheAttribute>()
                                    .FirstOrDefault();

                if (cacheMetadata != null)
                {
                    var currentUserEmail = context.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "email")?.Value ?? string.Empty;
                    var cacheName = cacheMetadata.CacheName ?? $"{currentUserEmail}|{context.HttpContext.Request.Path}{context.HttpContext.Request.QueryString}";
                    var expiredIn = cacheMetadata.ExpiredIn ?? TimeSpan.FromSeconds(_appDataMemoryCacheInSeconds);

                    if (!_memoryCache.TryGetValue(cacheName, out IActionResult cachedData))
                    {
                        _memoryCache.GetOrCreate(cacheName, entry =>
                        {
                            entry.AbsoluteExpirationRelativeToNow = expiredIn;
                            return context.Result;
                        });
                    }
                }
            }
        }
    }
}
