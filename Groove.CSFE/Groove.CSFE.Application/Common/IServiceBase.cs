using Groove.CSFE.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.Common
{
    public interface IServiceBase<TModel, TViewModel> : IDisposable
    {
        Task<TViewModel> GetByKeysAsync(params object[] keys);

        Task<DataSourceResult> ListAsync(DataSourceRequest request);

        Task<DataSourceResult> GetListAsync(DataSourceRequest request, Func<IQueryable<TModel>, IQueryable<TModel>> includes = null);

        Task<IEnumerable<TViewModel>> GetAllAsync();

        Task<TViewModel> CreateAsync(TViewModel viewModel);

        Task<TViewModel> UpdateAsync(TViewModel viewModel);

        Task<TViewModel> UpdateAsync(TViewModel viewModel, params object[] keys);

        Task<bool> DeleteAsync(TViewModel viewModel);

        Task<bool> DeleteAsync(params object[] keys);
    }
}
