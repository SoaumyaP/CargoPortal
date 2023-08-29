using Groove.SP.Application.Common;
using Groove.SP.Application.CruiseOrders.ViewModels;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using Groove.SP.Core.Entities.Cruise;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.Application.CruiseOrders.Services.Interfaces
{
    public interface ICruiseOrderService : IServiceBase<CruiseOrderModel, CreateCruiseOrderViewModel>
    {
        Task<CruiseOrderViewModel> CreateAsync(CreateCruiseOrderViewModel model, string userName);
        Task<IEnumerable<CruiseOrderViewModel>> CreateBulkAsync(IEnumerable<CreateCruiseOrderViewModel> model, string userName);
        Task<CruiseOrderViewModel> UpdateAsync(long cruiseOrderId, UpdateCruiseOrderViewModel model, string userName);
        Task DeleteAsync(long cruiseOrderId);
        Task<CruiseOrderViewModel> GetAsync(long cruiseOrderId);
    }
}
