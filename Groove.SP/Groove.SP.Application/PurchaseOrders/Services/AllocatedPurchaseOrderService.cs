using Groove.SP.Application.Activity.Services.Interfaces;
using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.PurchaseOrders.Services.Interfaces;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.PurchaseOrders.Services
{
    public class AllocatedPurchaseOrderService : IAllocatedPurchaseOrderService
    {
        private readonly IShipmentRepository _shipmentRepository;
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IActivityService _activityService;

        public AllocatedPurchaseOrderService(IRepository<ShipmentModel> shipmentRepository,
            IRepository<PurchaseOrderModel> purchaseOrderRepository,
            IUnitOfWorkProvider unitOfWorkProvider,
            IActivityService activityService)
        {
            _shipmentRepository = (IShipmentRepository)shipmentRepository;
            _purchaseOrderRepository = (IPurchaseOrderRepository)purchaseOrderRepository;
            _unitOfWork = unitOfWorkProvider.CreateUnitOfWork();
            _activityService = activityService;
        }

        public async Task<IList<PurchaseOrderModel>> GetListByShipmentAsync(long shipmentId)
        {
            var shipment = await _shipmentRepository.GetAsNoTrackingAsync(x => x.Id == shipmentId,
                null,
                x => x.Include(m => m.POFulfillmentAllocatedOrders).AsNoTracking());

            if (shipment == null)
            {
                throw new AppEntityNotFoundException($"Object Shipment with the id {shipmentId} not found!");
            }

            var purchaseOrderIdList = shipment.POFulfillmentAllocatedOrders.Select(x => x.PurchaseOrderId).Distinct();
            return _purchaseOrderRepository.QueryAsNoTracking(po => purchaseOrderIdList.Any(id => po.Id == id)).ToList();
        }

        public async Task ChangeStageToClosedAsync(long shipmentId, string userName, DateTime eventDate, string location = null)
        {
            var eventList = new List<ActivityViewModel>();
            var purchaseOrderList = await GetListByShipmentAsync(shipmentId);            foreach(var po in purchaseOrderList)
            {
                po.Stage = POStageType.Closed;

                var event1010 = new ActivityViewModel
                {
                    ActivityCode = Event.EVENT_1010,
                    PurchaseOrderId = po.Id,
                    ActivityDate = eventDate,
                    Location = location,
                    CreatedBy = userName
                };
                eventList.Add(event1010);
            }

            await _activityService.TriggerEventList(eventList);
            _purchaseOrderRepository.UpdateRange(purchaseOrderList.ToArray());
            await _unitOfWork.SaveChangesAsync();        }

        public async Task RevertStageToShipmentDispatchAsync(long shipmentId, string userName)
        {
            var purchaseOrderList = await GetListByShipmentAsync(shipmentId);

            foreach (var po in purchaseOrderList)
            {
                if (po.Stage == POStageType.Closed)
                {
                    po.Stage = POStageType.ShipmentDispatch;
                }
            }

            _purchaseOrderRepository.UpdateRange(purchaseOrderList.ToArray());
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
