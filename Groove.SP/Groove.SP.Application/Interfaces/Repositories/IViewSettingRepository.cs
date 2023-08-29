using Groove.SP.Core.Entities;
using System.Linq.Expressions;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.Application.Interfaces.Repositories
{
    public interface IViewSettingRepository : IRepository<ViewSettingModel>
    {
        Task<IEnumerable<ViewSettingModel>> QueryAsNoTrackingAsync(
            Expression<Func<ViewSettingModel, bool>> filter = null,
            Func<IQueryable<ViewSettingModel>, IOrderedQueryable<ViewSettingModel>> orderBy = null,
            Func<IQueryable<ViewSettingModel>, IQueryable<ViewSettingModel>> includes = null);
    }
}