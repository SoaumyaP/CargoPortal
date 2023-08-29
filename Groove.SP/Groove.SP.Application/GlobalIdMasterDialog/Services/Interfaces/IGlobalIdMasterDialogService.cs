using Groove.SP.Application.Common;
using Groove.SP.Application.GlobalIdMasterDialog.ViewModels;
using Groove.SP.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.Application.GlobalIdMasterDialog.Services.Interfaces
{
    public interface IGlobalIdMasterDialogService : IServiceBase<GlobalIdMasterDialogModel, GlobalIdMasterDialogViewModel>
    {
        Task<IEnumerable<GlobalIdMasterDialogViewModel>> GetByPurchaseOrderAsync(long poId);
        Task<IEnumerable<GlobalIdMasterDialogViewModel>> GetByPOFulfillmentAsync(long poffId);
        Task<IEnumerable<GlobalIdMasterDialogViewModel>> GetByShipmentAsync(long shipmentId);
        Task<IEnumerable<GlobalIdMasterDialogViewModel>> GetByRoutingOrderIdAsync(long routingOrderId);
    }
}
