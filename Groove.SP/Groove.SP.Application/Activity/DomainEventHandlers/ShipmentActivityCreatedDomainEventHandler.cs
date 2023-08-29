using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.POFulfillment.Services;
using Groove.SP.Application.POFulfillment.Services.Interfaces;
using Groove.SP.Application.PurchaseOrders.Services.Interfaces;
using Groove.SP.Application.Shipments.Services.Interfaces;
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
    public class ShipmentActivityCreatedDomainEventHandler : INotificationHandler<ActivityCreatedDomainEvent>
    {
        private readonly IPOFulfillmentService _poFulfillmentService;
        private readonly IShipmentService _shipmentService;
        private readonly IShipmentRepository _shipmentRepository;
        private readonly IAllocatedPurchaseOrderService _allocatedPurchaseOrderService;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly IPOFulfillmentOrderRepository _poFulfillmentOrderRepository;

        public ShipmentActivityCreatedDomainEventHandler(IPOFulfillmentService poFulfillmentService,
            IShipmentService shipmentService,
            IAllocatedPurchaseOrderService allocatedPurchaseOrderService,
            IPurchaseOrderService purchaseOrderService,
            IRepository<ShipmentModel> shipmentRepository,
            IPOFulfillmentOrderRepository poFulfillmentOrderRepository)
        {
            _poFulfillmentService = poFulfillmentService;
            _shipmentService = shipmentService;
            _allocatedPurchaseOrderService = allocatedPurchaseOrderService;
            _shipmentRepository = (IShipmentRepository)shipmentRepository;
            _purchaseOrderService = purchaseOrderService;
            _poFulfillmentOrderRepository = poFulfillmentOrderRepository;
        }

        public async Task Handle(ActivityCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            if (!notification.ShipmentId.HasValue)
            {
                return;
            }

            var shipment = await _shipmentRepository.GetAsNoTrackingAsync(s => s.Id == notification.ShipmentId,
                null,
                s => s.Include(x => x.POFulfillment).Include(c => c.CargoDetails)
                .Include(c => c.ConsignmentItineraries).ThenInclude(c => c.Itinerary).ThenInclude(c => c.FreightScheduler)
                );

            // Handle for shipment is linked with PO by CargoDetails without booking module
            // 1. Shipment.PofulfillmentId = null
            // 2. CargoDetails.OrderId is not existing in POfulfillmentOrders

            var shipmentGlobalActivity = notification.Activity.GlobalIdActivities
                .FirstOrDefault(ga => ga.GlobalId.StartsWith(EntityType.Shipment));

            if (shipment.CargoDetails.Any() && shipment.POFulfillment == null && shipment.OrderType == OrderType.Freight
                && !string.IsNullOrEmpty(shipment.ServiceType) && shipment.ServiceType.Contains("to-Door", StringComparison.OrdinalIgnoreCase)
                && shipment.Status.Equals(StatusType.ACTIVE, StringComparison.OrdinalIgnoreCase)
                && notification.Activity.ActivityCode == Event.EVENT_2054)
            {

                var poIdsWithoutBooking = shipment.CargoDetails.Select(c => c.OrderId.Value).Distinct().ToList();
                var poIdsBookedByBookings = await _poFulfillmentOrderRepository.QueryAsNoTracking(c => poIdsWithoutBooking.Contains(c.PurchaseOrderId)).ToListAsync();
                poIdsWithoutBooking = poIdsWithoutBooking.Where(c => !poIdsBookedByBookings.Any(s => s.PurchaseOrderId == c)).ToList();

                var isShipmentDeparted = shipment.ConsignmentItineraries.Select(c => c.Itinerary).Select(c => c.FreightScheduler).Any(c => c.ATDDate.HasValue);
                var numberOfEvent2054 = await _shipmentService.CountNumberOfEvent2054Async(shipment.Id);
                if (isShipmentDeparted == false && numberOfEvent2054 == 0 && poIdsWithoutBooking.Any())
                {
                    _purchaseOrderService.AdjustQuantityOnPOLineItems(new List<long>() { shipment.Id }, poIdsWithoutBooking, AdjustBalanceOnPOLineItemsType.Deduct);
                }
                await _purchaseOrderService.ChangeStageToCloseAsync(poIdsWithoutBooking, AppConstant.SYSTEM_USERNAME, notification.Activity.ActivityDate, notification.Activity.Location, notification.Activity.Remark);
            }

            if (shipment == null || !shipment.POFulfillmentId.HasValue)
            {
                return;
            }

            #region 2054 : SM - Shipment handover to consignee
            if (!string.IsNullOrEmpty(shipment.ServiceType) && shipment.ServiceType.Contains("to-Door", StringComparison.OrdinalIgnoreCase)
                && shipment.Status.Equals(StatusType.ACTIVE, StringComparison.OrdinalIgnoreCase)
                && notification.Activity.ActivityCode == Event.EVENT_2054)
            {
                bool isChildShipment = shipment.POFulfillment.FulfilledFromPOType == POType.Blanket;

                if (isChildShipment)
                {
                    await _allocatedPurchaseOrderService.ChangeStageToClosedAsync(shipment.Id, AppConstant.SYSTEM_USERNAME, notification.Activity.ActivityDate, notification.Activity.Location);

                    bool isOtherChildShipmentsClosed = await _shipmentService.IsOtherChildShipmentsClosed(shipment.POFulfillmentId.Value, shipment.Id);
                    if (isOtherChildShipmentsClosed)
                    {
                        await _poFulfillmentService.ChangeStageToClosedAsync(
                            new List<long> { shipment.POFulfillmentId.Value },
                            AppConstant.SYSTEM_USERNAME,
                            notification.Activity.ActivityDate,
                            notification.Activity.Location,
                            notification.Activity.Remark);
                    }
                    return;
                }

                await _poFulfillmentService.ChangeStageToClosedAsync(
                    new List<long> { shipment.POFulfillmentId.Value },
                    AppConstant.SYSTEM_USERNAME,
                    notification.Activity.ActivityDate,
                    notification.Activity.Location,
                    notification.Activity.Remark);
                return;
            }
            #endregion
        }
    }
}
