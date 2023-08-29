using Groove.SP.Application.Common;
using Groove.SP.Application.CruiseOrders.ViewModels;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities.Cruise;
using System.Threading.Tasks;

namespace Groove.SP.Application.CruiseOrders.Services.Interfaces
{
    public interface ICruiseOrderListService : IServiceBase<CruiseOrderModel, CruiseOrderListViewModel>
    {
        Task<DataSourceResult> ListAsync(DataSourceRequest request, bool isInternal, long? organizationId = 0);
    }
}
