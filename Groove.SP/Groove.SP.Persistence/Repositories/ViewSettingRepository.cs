using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities;
using Groove.SP.Persistence.Contexts;
using Groove.SP.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Groove.SP.Persistence.Repositories
{
    public class ViewSettingRepository : Repository<SpContext, ViewSettingModel>, IViewSettingRepository
    {
        public ViewSettingRepository(SpContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ViewSettingModel>> QueryAsNoTrackingAsync(Expression<Func<ViewSettingModel, bool>> filter = null, Func<IQueryable<ViewSettingModel>, IOrderedQueryable<ViewSettingModel>> orderBy = null, Func<IQueryable<ViewSettingModel>, IQueryable<ViewSettingModel>> includes = null)
        {
            return await base.QueryAsNoTracking(filter, orderBy, includes).ToListAsync();
        }
    }
}
