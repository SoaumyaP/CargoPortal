using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace Groove.SP.Application.Caching
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache memoryCache;

        public MemoryCacheService(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }

        public T Get<T>(string key)
        {
            this.memoryCache.TryGetValue(key, out T value);
            return value;
        }

        public Task<T> GetAsync<T>(string key)
        {
            return Task.Run<T>(() => Get<T>(key));
        }

        public void Remove(string key)
        {
            this.memoryCache.Remove(key);
        }

        public Task RemoveAsync(string key)
        {
            return Task.Run(() => Remove(key));
        }

        public void Set<T>(string key, T value, TimeSpan? duration = null)
        {
            this.memoryCache.Set(key, value);
        }
    }
}
