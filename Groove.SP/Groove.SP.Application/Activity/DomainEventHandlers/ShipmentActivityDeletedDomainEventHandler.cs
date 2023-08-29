using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.POFulfillment.Services;
using Groove.SP.Application.POFulfillment.Services.Interfaces;
using Groove.SP.Application.PurchaseOrders.Services.Interfaces;
using Groove.SP.Application.Shipments.Services.Interfaces;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Events;
using Groove.SP.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Groove.SP.Application.Activity.DomainEventHandlers
{
    public class ShipmentActivityDeletedDomainEventHandler : INotificationHandler<ActivityDeletedDomainEvent>
    {
        private readonly IPOFulfillmentService _poFulfillmentService;
        private readonly IShipmentService _shipmentService;
        private readonly IShipmentRepository _shipmentRepository;
        private readonly IAllocatedPurchaseOrderService _allocatedPurchaseOrderService;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly IPOFulfillmentOrderRepository _poFulfillmentOrderRepository;

        public ShipmentActivityDeletedDomainEventHandler(IPOFulfillmentService poFulfillmentService,
            IShipmentService shipmentService,
            IAllocatedPurchaseOrderService allocatedPurchaseOrderService,
            IPurchaseOrderService purchaseOrderService,
            IRepository<ShipmentModel> shipmentRepository, 
            IPOFulfillmentOrderRepository poFulfillmentOrderRepository)
        {
            _poFulfillmentService = poFulfillmentService;
            _shipmentService = shipmentService;
            _purchaseOrderService = purchaseOrderService;
            _allocatedPurchaseOrderService = allocatedPurchaseOrderService;
            _shipmentRepository = (IShipmentRepository)shipmentRepository;
            _poFulfillmentOrderRepository = poFulfillmentOrderRepository;
        }

        public async Task Handle(ActivityDeletedDomainEvent notification, CancellationToken cancellationToken)
        {
            var shipmentGlobalActivity = notification.Activity.GlobalIdActivities
                .FirstOrDefault(ga => ga.GlobalId.StartsWith(EntityType.Shipment));
            var shipmentId = shipmentGlobalActivity == null ? null : CommonHelper.GetEntityId(shipmentGlobalActivity.GlobalId, EntityType.Shipment);

            if (!shipmentId.HasValue)
            {
                return;
            }

            var shipment = await _shipmentRepository.GetAsNoTrackingAsync(x => x.Id == shipmentId,
                null,
                x => x.Include(s => s.POFulfillment).Include(c => c.CargoDetails)
                    .Include(c => c.ConsignmentItineraries).ThenInclude(c => c.Itinerary).ThenInclude(c => c.FreightScheduler)
                );

            // Handle for shipment is linked with PO by CargoDetails without booking module
            // 1. Shipment.PofulfillmentId = null
            // 2. CargoDetails.OrderId is not existing in POfulfillmentOrders

            if (shipment.CargoDetails.Any() && shipment.POFulfillment == null && shipment.OrderType == OrderType.Freight
                && !string.IsNullOrEmpty(shipment.ServiceType) && shipment.ServiceType.Contains("to-Door", StringComparison.OrdinalIgnoreCase)
                && shipment.Status.Equals(StatusType.ACTIVE, StringComparison.OrdinalIgnoreCase)
                && notification.Activity.ActivityCode == Event.EVENT_2054)
            {
                var poIdsWithoutBooking = shipment.CargoDetails.Select(c => c.OrderId.Value).Distinct().ToList();
                var poIdsBookedByBookings = await _poFulfillmentOrderRepository.QueryAsNoTracking(c => poIdsWithoutBooking.Contains(c.PurchaseOrderId)).ToListAsync();
                poIdsWithoutBooking = poIdsWithoutBooking.Where(c => !poIdsBookedByBookings.Any(s => s.PurchaseOrderId == c)).ToList();

                var numberOfEvent2054 = await _shipmentService.CountNumberOfEvent2054Async(shipment.Id);
                if (numberOfEvent2054 == 1 && poIdsWithoutBooking.Any())
                {
                    await _purchaseOrderService.RevertStageToShipmentDispatchAsync(poIdsWithoutBooking);
                    var isShipmentDeparted = shipment.ConsignmentItineraries.Select(c => c.Itinerary).Select(c => c.FreightScheduler).Any(c => c.ATDDate.HasValue);
                    if (isShipmentDeparted == false)
                    {
                        _purchaseOrderService.AdjustQuantityOnPOLineItems(new List<long>() { shipment.Id }, poIdsWithoutBooking, AdjustBalanceOnPOLineItemsType.Return);
                        await _purchaseOrderService.RevertStageToReleasedAsync(poIdsWithoutBooking);
                    }
                }
            }

            if (!shipment.POFulfillmentId.HasValue)
            {
                return;
            }

            #region 2054 : SM - Shipment handover to consignee
            if (shipment.ServiceType.Contains("to-Door", StringComparison.OrdinalIgnoreCase)
                && shipment.Status.Equals(StatusType.ACTIVE, StringComparison.OrdinalIgnoreCase)
                && notification.Activity.ActivityCode == Event.EVENT_2054)
            {
                bool isChildShipment = shipment.POFulfillment.FulfilledFromPOType == POType.Blanket;

                if (isChildShipment)
                {
                    await _allocatedPurchaseOrderService.RevertStageToShipmentDispatchAsync(shipment.Id, AppConstant.SYSTEM_USERNAME);

                    bool isOtherChildShipmentsClosed = await _shipmentService.IsOtherChildShipmentsClosed(shipment.POFulfillmentId.Value, shipment.Id);
                    if (isOtherChildShipmentsClosed)
                    {
                        await _poFulfillmentService.RevertStageToShipmentDispatchAsync(new List<long> { shipment.POFulfillmentId.Value }, AppConstant.SYSTEM_USERNAME);
                    }
                    return;
                }

                await _poFulfillmentService.RevertStageToShipmentDispatchAsync(new List<long> { shipment.POFulfillmentId.Value }, AppConstant.SYSTEM_USERNAME);
                return;
            }
            #endregion
        }
    }
}

