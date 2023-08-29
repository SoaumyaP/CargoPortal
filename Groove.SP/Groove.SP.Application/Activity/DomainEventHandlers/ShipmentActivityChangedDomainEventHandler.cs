using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
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
    public class ShipmentActivityChangedDomainEventHandler : INotificationHandler<ActivityChangedDomainEvent>
    {
        private readonly IPOFulfillmentService _poFulfillmentService;
        private readonly IShipmentService _shipmentService;
        private readonly IAllocatedPurchaseOrderService _allocatedPurchaseOrderService;
        private readonly IShipmentRepository _shipmentRepository;
        private readonly IActivityRepository _activityRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly IPOFulfillmentOrderRepository _poFulfillmentOrderRepository;

        public ShipmentActivityChangedDomainEventHandler(IPOFulfillmentService poFulfillmentService,
            IShipmentService shipmentService,
            IAllocatedPurchaseOrderService allocatedPurchaseOrderService,
            IRepository<ShipmentModel> shipmentRepository,
            IRepository<ActivityModel> activityRepository,
            IPurchaseOrderService purchaseOrderService,
            IPOFulfillmentOrderRepository poFulfillmentOrderRepository,
            IUnitOfWorkProvider unitOfWorkProvider)
        {
            _shipmentRepository = (IShipmentRepository)shipmentRepository;
            _poFulfillmentService = poFulfillmentService;
            _allocatedPurchaseOrderService = allocatedPurchaseOrderService;
            _shipmentService = shipmentService;
            _purchaseOrderService = purchaseOrderService;
            _activityRepository = (IActivityRepository)activityRepository;
            _poFulfillmentOrderRepository = poFulfillmentOrderRepository;
            _unitOfWork = unitOfWorkProvider.CreateUnitOfWork();
        }

        public async Task Handle(ActivityChangedDomainEvent notification, CancellationToken cancellationToken)
        {
            if (!notification.CurrentShipmentId.HasValue)
            {
                return;
            }

            var shipment = await _shipmentRepository.GetAsNoTrackingAsync(s => s.Id == notification.CurrentShipmentId.Value,
                null,
                s => s.Include(x => x.POFulfillment).Include(c => c.CargoDetails)
                .Include(c => c.ConsignmentItineraries).ThenInclude(c => c.Itinerary).ThenInclude(c => c.FreightScheduler)
                );


            // Handle for shipment is linked with PO by CargoDetails without booking module
            // 1. Shipment.PofulfillmentId = null
            // 2. CargoDetails.OrderId is not existing in POfulfillmentOrders

            if (shipment.CargoDetails.Any() && shipment.POFulfillment == null && shipment.OrderType == OrderType.Freight
                && !string.IsNullOrEmpty(shipment.ServiceType) && shipment.ServiceType.Contains("to-Door", StringComparison.OrdinalIgnoreCase)
                && shipment.Status.Equals(StatusType.ACTIVE, StringComparison.OrdinalIgnoreCase))
            {
                var poIdsWithoutBooking = shipment.CargoDetails.Select(c => c.OrderId.Value).Distinct().ToList();
                var poIdsBookedByBookings = await _poFulfillmentOrderRepository.QueryAsNoTracking(c => poIdsWithoutBooking.Contains(c.PurchaseOrderId)).ToListAsync();
                poIdsWithoutBooking = poIdsWithoutBooking.Where(c => !poIdsBookedByBookings.Any(s => s.PurchaseOrderId == c)).ToList();
                var isShipmentDeparted = shipment.ConsignmentItineraries.Select(c => c.Itinerary).Select(c => c.FreightScheduler).Any(c => c.ATDDate.HasValue);
                var numberOfEvent2054 = await _shipmentService.CountNumberOfEvent2054Async(shipment.Id);

                // Create event #2054
                if (notification.PreviousActivityCode != Event.EVENT_2054 && notification.CurrentActivityCode == Event.EVENT_2054)
                {
                    if (isShipmentDeparted == false && numberOfEvent2054 == 0 && poIdsWithoutBooking.Any())
                    {
                        _purchaseOrderService.AdjustQuantityOnPOLineItems(new List<long>() { shipment.Id }, poIdsWithoutBooking, AdjustBalanceOnPOLineItemsType.Deduct);
                    }
                    await _purchaseOrderService.ChangeStageToCloseAsync(poIdsWithoutBooking, AppConstant.SYSTEM_USERNAME, notification.CurrentActivityDate, notification.CurrentLocation, notification.CurrentRemark);
                }

                // Delete event #2054
                if (notification.PreviousActivityCode == Event.EVENT_2054 && notification.CurrentActivityCode != Event.EVENT_2054)
                {
                    if (numberOfEvent2054 == 1 && poIdsWithoutBooking.Any())
                    {
                        await _purchaseOrderService.RevertStageToShipmentDispatchAsync(poIdsWithoutBooking);
                        if (isShipmentDeparted == false)
                        {
                            _purchaseOrderService.AdjustQuantityOnPOLineItems(new List<long>() { shipment.Id }, poIdsWithoutBooking, AdjustBalanceOnPOLineItemsType.Return);
                            await _purchaseOrderService.RevertStageToReleasedAsync(poIdsWithoutBooking);
                        }
                    }

                    return;
                }

                // Update event #2054
                if (notification.PreviousActivityCode == Event.EVENT_2054 && notification.CurrentActivityCode == Event.EVENT_2054)
                {
                    // Update info of event 1010
                    await _purchaseOrderService.ChangeStageToCloseAsync(poIdsWithoutBooking, AppConstant.SYSTEM_USERNAME, notification.CurrentActivityDate, notification.CurrentLocation, notification.CurrentRemark);
                }
            }

            if (shipment == null || !shipment.POFulfillmentId.HasValue)
            {
                return;
            }

            // Change event #2054 to another event
            if (shipment.ServiceType.Contains("to-Door", StringComparison.OrdinalIgnoreCase)
                    && shipment.Status.Equals(StatusType.ACTIVE, StringComparison.OrdinalIgnoreCase)
                    && notification.PreviousActivityCode == Event.EVENT_2054 && notification.CurrentActivityCode != Event.EVENT_2054)
            {
                await _poFulfillmentService.RevertStageToShipmentDispatchAsync(new List<long> { shipment.POFulfillmentId.Value }, AppConstant.SYSTEM_USERNAME);
                return;
            }

            // Change event to event #2054
            if (shipment.ServiceType.Contains("to-Door", StringComparison.OrdinalIgnoreCase)
                    && shipment.Status.Equals(StatusType.ACTIVE, StringComparison.OrdinalIgnoreCase)
                    && notification.PreviousActivityCode != Event.EVENT_2054 && notification.CurrentActivityCode == Event.EVENT_2054)
            {
                bool isChildShipment = shipment.POFulfillment.FulfilledFromPOType == POType.Blanket;

                if (isChildShipment)
                {
                    await _allocatedPurchaseOrderService.ChangeStageToClosedAsync(shipment.Id, AppConstant.SYSTEM_USERNAME, notification.CurrentActivityDate, notification.CurrentLocation);

                    bool isOtherChildShipmentsClosed = await _shipmentService.IsOtherChildShipmentsClosed(shipment.POFulfillmentId.Value, shipment.Id);
                    if (isOtherChildShipmentsClosed)
                    {
                        await _poFulfillmentService.ChangeStageToClosedAsync(new List<long> { shipment.POFulfillmentId.Value },
                            AppConstant.SYSTEM_USERNAME,
                            notification.CurrentActivityDate,
                            notification.CurrentLocation,
                            notification.CurrentRemark);
                    }
                    return;
                }

                await _poFulfillmentService.ChangeStageToClosedAsync(new List<long> { shipment.POFulfillmentId.Value },
                    AppConstant.SYSTEM_USERNAME,
                    notification.CurrentActivityDate,
                    notification.CurrentLocation,
                    notification.CurrentRemark);
                return;
            }

            // Update event #2054
            if (shipment.ServiceType.Contains("to-Door", StringComparison.OrdinalIgnoreCase)
                    && shipment.Status.Equals(StatusType.ACTIVE, StringComparison.OrdinalIgnoreCase)
                    && notification.PreviousActivityCode == Event.EVENT_2054 && notification.CurrentActivityCode == Event.EVENT_2054)
            {
                bool isChildShipment = shipment.POFulfillment.FulfilledFromPOType == POType.Blanket;

                if (isChildShipment)
                {
                    await UpdateActivityOnAllocatedPO(Event.EVENT_1010, notification.CurrentActivityDate, notification.CurrentLocation, shipment.Id);
                    return;
                }
                await _poFulfillmentService.ChangeStageToClosedAsync(new List<long> { shipment.POFulfillmentId.Value },
                    AppConstant.SYSTEM_USERNAME,
                    notification.CurrentActivityDate,
                    notification.CurrentLocation,
                    notification.CurrentRemark);
                return;
            }
        }

        private async Task UpdateActivityOnAllocatedPO(string eventCode, DateTime activityDate, string location, long shipmentId)
        {
            var purchaseOrderList = await _allocatedPurchaseOrderService.GetListByShipmentAsync(shipmentId);

            foreach (var po in purchaseOrderList)
            {
                var globalId = CommonHelper.GenerateGlobalId(po.Id, EntityType.CustomerPO);
                var activities = await _activityRepository.Query(a => a.GlobalIdActivities.Any(g => g.GlobalId == globalId) && a.ActivityCode == eventCode,
                    null,
                    i => i.Include(a => a.GlobalIdActivities)).ToListAsync();

                foreach (var activity in activities)
                {
                    activity.ActivityDate = activityDate;
                    activity.Location = location;
                    activity.Audit(AppConstant.SYSTEM_USERNAME);

                    foreach (var globalIdActivity in activity.GlobalIdActivities)
                    {
                        globalIdActivity.ActivityDate = activity.ActivityDate;
                        globalIdActivity.Location = activity.Location;
                        globalIdActivity.Audit(AppConstant.SYSTEM_USERNAME);
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
