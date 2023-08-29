using System;

namespace Groove.SP.API.Filters
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class AppDataMemoryCacheAttribute : Attribute
    {
        public string CacheName { get; set; }
        public TimeSpan? ExpiredIn { get; set; }

        /// <summary>
        /// To enable data caching on memory. <b>Each user has own cache as default.</b>
        /// </summary>
        /// <param name="cacheName">Unique name of cache. <b>If null/empty, cache is separated by user login email.</b></param>
        /// <param name="expriedInSeconds">Cache will expire in a number of seconds</param>
        public AppDataMemoryCacheAttribute(string cacheName, int expriedInSeconds)
        {
            CacheName = cacheName;
            ExpiredIn = TimeSpan.FromSeconds(expriedInSeconds);
        }

        public AppDataMemoryCacheAttribute()
        {
            CacheName = null;
            ExpiredIn = null;
        }
    }
}
