using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.POFulfillment.Services;
using Groove.SP.Application.POFulfillment.Services.Interfaces;
using Groove.SP.Application.PurchaseOrders.Services.Interfaces;
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
    public class FreightSchedulerActivityCreatedDomainEventHandler : INotificationHandler<ActivityCreatedDomainEvent>
    {
        private readonly IPOFulfillmentService _poFulfillmentService;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly IRepository<ConsignmentItineraryModel> _consignmentItineraryRepository;
        private readonly IRepository<FreightSchedulerModel> _freightSchedulerRepository;
        private readonly IPOFulfillmentOrderRepository _poFulfillmentOrderRepository;

        public FreightSchedulerActivityCreatedDomainEventHandler(IPOFulfillmentService poFulfillmentService,
            IRepository<ConsignmentItineraryModel> consignmentItineraryRepository,
            IPurchaseOrderService purchaseOrderService,
            IRepository<FreightSchedulerModel> freightSchedulerRepository,
            IPOFulfillmentOrderRepository poFulfillmentOrderRepository)
        {
            _poFulfillmentService = poFulfillmentService;
            _consignmentItineraryRepository = consignmentItineraryRepository;
            _freightSchedulerRepository = freightSchedulerRepository;
            _purchaseOrderService = purchaseOrderService;
            _poFulfillmentOrderRepository = poFulfillmentOrderRepository;
        }
    

        public async Task Handle(ActivityCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            if (!notification.FreightSchedulerId.HasValue)
            {
                return;
            }

            var freightScheduler = await _freightSchedulerRepository.GetAsNoTrackingAsync(x => x.Id == notification.FreightSchedulerId.Value);
            if (freightScheduler == null)
            {
                return;
            }

            var consignmentItineraries = await _consignmentItineraryRepository.QueryAsNoTracking(ci => ci.Itinerary.ScheduleId == notification.FreightSchedulerId.Value && ci.ShipmentId != null, null,
                 x => x.Include(ci => ci.Shipment)
                 .ThenInclude(s => s.POFulfillment)
                 .Include(c => c.Shipment).ThenInclude(c => c.CargoDetails)
                 ).ToListAsync();

            var shipments = consignmentItineraries.Select(x => x.Shipment).Distinct();

            if (shipments == null || !shipments.Any())
            {
                return;
            }

            try
            {
                //Handle for shipment is linked with PO by CargoDetails without booking module
                // 1. Shipment.PofulfillmentId = null
                // 2. CargoDetails.OrderId is not existing in POfulfillmentOrders

                var shipmentsLinkedPOWithoutBooking = shipments.Where(c => c.CargoDetails.Any() && c.POFulfillmentId == null && c.OrderType == OrderType.Freight).ToList();
                var poIdsWithoutBooking = shipmentsLinkedPOWithoutBooking.SelectMany(c => c.CargoDetails).Where(x => x.OrderId != null).Select(c => c.OrderId.Value).Distinct().ToList();
                var poIdsBookedByBookings = await _poFulfillmentOrderRepository.QueryAsNoTracking(c => poIdsWithoutBooking.Contains(c.PurchaseOrderId)).ToListAsync();
                poIdsWithoutBooking = poIdsWithoutBooking.Where(c => !poIdsBookedByBookings.Any(s => s.PurchaseOrderId == c)).ToList();

                var cargoDetails = await _purchaseOrderService.GetCargoDetails(poIdsWithoutBooking);
                var shipmentsLinkedPO = cargoDetails.Select(c => c.Shipment).DistinctBy(c => c.Id).ToList();
                var consignmentItinerariesFSWithATD = await _consignmentItineraryRepository.QueryAsNoTracking(
                       c => shipmentsLinkedPO.Select(s => s.Id).ToList().Contains(c.ShipmentId ?? 0)
                           && c.Itinerary.FreightScheduler.ATDDate != null, null,
                       c => c.Include(c => c.Itinerary).ThenInclude(c => c.FreightScheduler)
                       ).ToListAsync();

                #region 7001 : VA - Vessel Departure / 7003 : VA - Flight Departure
                if (notification.Activity.ActivityCode == Event.EVENT_7001 || notification.Activity.ActivityCode == Event.EVENT_7003)
                {
                    var groupedByShipmentId = consignmentItinerariesFSWithATD.GroupBy(c => c.ShipmentId).ToList().Where(c => c.Count() > 1);
                    var shipmentIdsNotUpdate = groupedByShipmentId.Select(c => c.Key).ToList();

                    //PO#1 - Shipment#1
                    //Shipment#1 has n FS
                    shipmentsLinkedPOWithoutBooking = shipmentsLinkedPOWithoutBooking.Where(c => !shipmentIdsNotUpdate.Any(s => s == c.Id)).ToList();
                    var ids = shipmentsLinkedPOWithoutBooking.Select(c => c.Id);
                    if (ids.Any() && poIdsWithoutBooking.Any() && notification.MetaData != "createdfromitineraryapi")
                    {
                        _purchaseOrderService.AdjustQuantityOnPOLineItems(ids, poIdsWithoutBooking, AdjustBalanceOnPOLineItemsType.Deduct);
                    }

                    await _purchaseOrderService.ChangeStageToShipmentDispatchAsync(poIdsWithoutBooking);

                    List<long> poffIdListToDispatch = new();
                    shipments = shipments.Where(c => c.POFulfillment != null).ToList();

                    foreach (var shipment in shipments)
                    {
                        if (shipment.POFulfillment.FulfilledFromPOType != POType.Blanket)
                        {
                            poffIdListToDispatch.Add(shipment.POFulfillmentId.Value);
                        }
                        else
                        {
                            //TODO: handle for blanket booking
                        }
                    }

                    await _poFulfillmentService.ChangeStageToShipmentDispatchAsync(poffIdListToDispatch, AppConstant.SYSTEM_USERNAME);
                    return;
                }
                #endregion

                #region 7002 : VA - Vessel Arrival / 7004 : VA - Flight Arrival
                if (notification.Activity.ActivityCode == Event.EVENT_7002 || notification.Activity.ActivityCode == Event.EVENT_7004)
                {
                    // Handle for shipment is linked with PO by CargoDetails without booking module
                    // 1. Shipment.PofulfillmentId = null
                    // 2. CargoDetails.OrderId is not existing in POfulfillmentOrders

                    shipmentsLinkedPOWithoutBooking = shipmentsLinkedPOWithoutBooking
                           .Where(c =>
                          c.ServiceType != null && (c.ServiceType.Contains("to-Port", StringComparison.OrdinalIgnoreCase) || c.ServiceType.Contains("to-Airport", StringComparison.OrdinalIgnoreCase))
                          && freightScheduler.LocationToName.Equals(c.ShipTo ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                          && c.Status.Equals(StatusType.ACTIVE, StringComparison.OrdinalIgnoreCase)).ToList();
                    poIdsWithoutBooking = shipmentsLinkedPOWithoutBooking.SelectMany(c => c.CargoDetails).Where(x => x.OrderId != null).Select(c => c.OrderId.Value).Distinct().ToList();

                    // Handle for case add ATADate from FS api
                    if (freightScheduler.ATDDate == null && poIdsWithoutBooking.Any())
                    {
                        var freightSchedulesHaveATA = await _consignmentItineraryRepository.QueryAsNoTracking(
                       c => shipmentsLinkedPO.Select(s => s.Id).ToList().Contains(c.ShipmentId ?? 0)
                           && c.Itinerary.FreightScheduler.ATADate.HasValue, null,
                       c => c.Include(c => c.Itinerary).ThenInclude(c => c.FreightScheduler)
                       ).ToListAsync();

                        var groupedFreightSchedulesHaveATA = freightSchedulesHaveATA.GroupBy(c => c.ShipmentId).ToList().Where(c => c.Count() > 1);
                        var shipmentIdsNotUpdate = groupedFreightSchedulesHaveATA.Select(c => c.Key).ToList();

                        //PO#1 - Shipment#1
                        //Shipment#1 has n FS
                        shipmentsLinkedPOWithoutBooking = shipmentsLinkedPOWithoutBooking.Where(c => !shipmentIdsNotUpdate.Any(s => s == c.Id)).ToList();

                        var ids = shipmentsLinkedPOWithoutBooking.Select(c => c.Id);
                        if (ids.Any() && poIdsWithoutBooking.Any() && notification.MetaData != "createdfromitineraryapi")
                        {
                            _purchaseOrderService.AdjustQuantityOnPOLineItems(ids, poIdsWithoutBooking, AdjustBalanceOnPOLineItemsType.Deduct);
                        }
                    }
                    await _purchaseOrderService.ChangeStageToCloseAsync(poIdsWithoutBooking, AppConstant.SYSTEM_USERNAME, notification.Activity.ActivityDate, notification.Activity.Location);

                    List<long> poffIdListToClose = new();
                    shipments = shipments.Where(c => c.POFulfillment != null).ToList();

                    foreach (var shipment in shipments)
                    {
                        if (shipment.ServiceType != null && (shipment.ServiceType.Contains("to-Port", StringComparison.OrdinalIgnoreCase) || shipment.ServiceType.Contains("to-Airport", StringComparison.OrdinalIgnoreCase))
                            && freightScheduler.LocationToName.Equals(shipment.ShipTo ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                            && shipment.Status.Equals(StatusType.ACTIVE, StringComparison.OrdinalIgnoreCase))
                        {
                            if (shipment.POFulfillment.FulfilledFromPOType != POType.Blanket
                                && shipment.POFulfillment.Status == POFulfillmentStatus.Active)
                            {
                                poffIdListToClose.Add(shipment.POFulfillmentId.Value);
                            }
                            else
                            {
                                //TODO: handle for blanket booking
                            }
                        }
                    }

                    await _poFulfillmentService.ChangeStageToClosedAsync(
                        poffIdListToClose,
                        AppConstant.SYSTEM_USERNAME,
                        notification.Activity.ActivityDate,
                        notification.Activity.Location);

                    return;
                }
                #endregion
            }

            catch (Exception ex)
            {
                return;
            }
        }
    }
}
