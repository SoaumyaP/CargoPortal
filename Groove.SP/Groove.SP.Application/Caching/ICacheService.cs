using System;
using System.Threading.Tasks;

namespace Groove.SP.Application.Caching
{
    public interface ICacheService
    {
        void Set<T>(string key, T value, TimeSpan? duration = null);

        T Get<T>(string key);
        Task<T> GetAsync<T>(string key);

        void Remove(string key);
        Task RemoveAsync(string key);
    }
}
