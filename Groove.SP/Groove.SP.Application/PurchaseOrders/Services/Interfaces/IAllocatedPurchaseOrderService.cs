using Groove.SP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.Application.PurchaseOrders.Services.Interfaces
{
    public interface IAllocatedPurchaseOrderService
    {
        Task<IList<PurchaseOrderModel>> GetListByShipmentAsync(long shipmentId);
        Task ChangeStageToClosedAsync(long shipmentId, string userName, DateTime eventDate, string location = null);
        Task RevertStageToShipmentDispatchAsync(long shipmentId, string userName);
    }
}
