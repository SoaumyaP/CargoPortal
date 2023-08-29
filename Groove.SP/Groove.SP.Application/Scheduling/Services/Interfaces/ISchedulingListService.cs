using Groove.SP.Application.Common;
using Groove.SP.Application.Scheduling.ViewModels;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;

using System.Threading.Tasks;

namespace Groove.SP.Application.Scheduling.Services.Interfaces
{
    public interface ISchedulingListService : IServiceBase<SchedulingModel, SchedulingListViewModel>
    {
        Task<DataSourceResult> ListAsync(DataSourceRequest request, bool isInternal, long organizationId);
    }
}
