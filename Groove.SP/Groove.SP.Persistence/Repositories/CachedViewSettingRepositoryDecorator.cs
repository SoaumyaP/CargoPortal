using Groove.SP.Core.Entities;
using Groove.SP.Persistence.Contexts;
using Groove.SP.Persistence.Repositories.Base;
using Microsoft.Extensions.Caching.Memory;
using System.Linq.Expressions;
using System.Linq;
using System.Threading.Tasks;
using System;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Models;
using System.Collections.Generic;

namespace Groove.SP.Persistence.Repositories
{
    public class CachedViewSettingRepositoryDecorator : Repository<SpContext, ViewSettingModel>, IViewSettingRepository
    {
        private readonly IViewSettingRepository _viewSettingRepository;
        private readonly IMemoryCache _cache;
        private const string modelCacheKey = "ViewSettings";
        private MemoryCacheEntryOptions cacheOptions;

        // alternatively use IDistributedCache if the repository uses redis and multiple services
        public CachedViewSettingRepositoryDecorator(SpContext context, IViewSettingRepository viewSettingRepository, IMemoryCache cache) : base(context)
        {
            _viewSettingRepository = viewSettingRepository;
            _cache = cache;

            // 30 mins cache
            cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(relative: TimeSpan.FromSeconds(AppConstant.DEFAULT_VIEW_SETTING_CACHE_SECONDS));
        }

        public async Task<IEnumerable<ViewSettingModel>> QueryAsNoTrackingAsync(Expression<Func<ViewSettingModel, bool>> filter = null, Func<IQueryable<ViewSettingModel>, IOrderedQueryable<ViewSettingModel>> orderBy = null, Func<IQueryable<ViewSettingModel>, IQueryable<ViewSettingModel>> includes = null)
        {
            // cache key
            string key = modelCacheKey + "-" + filter?.ToString() + orderBy?.ToString() + includes?.ToString();

            return await _cache.GetOrCreate(key, entry =>
            {
                entry.SetOptions(cacheOptions);
                return _viewSettingRepository.QueryAsNoTrackingAsync(filter, orderBy, includes);
            });
        }
    }
}
