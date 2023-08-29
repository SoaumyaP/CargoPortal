using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.POFulfillment.Services;
using Groove.SP.Application.POFulfillment.Services.Interfaces;
using Groove.SP.Application.PurchaseOrders.Services.Interfaces;
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
    public class FreightSchedulerActivityDeletedDomainEventHandler : INotificationHandler<ActivityDeletedDomainEvent>
    {
        private readonly IPOFulfillmentService _poFulfillmentService;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly IRepository<ConsignmentItineraryModel> _consignmentItineraryRepository;
        private readonly IRepository<FreightSchedulerModel> _freightSchedulerRepository;
        private readonly IPOFulfillmentOrderRepository _poFulfillmentOrderRepository;

        public FreightSchedulerActivityDeletedDomainEventHandler(IPOFulfillmentService poFulfillmentService,
            IRepository<ConsignmentItineraryModel> consignmentItineraryRepository,
            IPurchaseOrderService purchaseOrderService,
            IRepository<FreightSchedulerModel> freightSchedulerRepository,
            IPOFulfillmentOrderRepository poFulfillmentOrderRepository)
        {
            _poFulfillmentService = poFulfillmentService;
            _consignmentItineraryRepository = consignmentItineraryRepository;
            _purchaseOrderService = purchaseOrderService;
            _freightSchedulerRepository = freightSchedulerRepository;
            _poFulfillmentOrderRepository = poFulfillmentOrderRepository;
        }

        public async Task Handle(ActivityDeletedDomainEvent notification, CancellationToken cancellationToken)
        {
            var freightSchedulerGlobalActivity = notification.Activity.GlobalIdActivities
                .FirstOrDefault(ga => ga.GlobalId.StartsWith(EntityType.FreightScheduler));
            var freightSchedulerId = freightSchedulerGlobalActivity == null ? null : CommonHelper.GetEntityId(freightSchedulerGlobalActivity.GlobalId, EntityType.FreightScheduler);

            if (!freightSchedulerId.HasValue)
            {
                return;
            }

            var freightScheduler = await _freightSchedulerRepository.GetAsNoTrackingAsync(x => x.Id == freightSchedulerId.Value);
            if (freightScheduler == null)
            {
                return;
            }

            var consignmentItineraries = await _consignmentItineraryRepository.QueryAsNoTracking(ci => ci.Itinerary.ScheduleId == freightSchedulerId && ci.ShipmentId != null, null,
                 x => x.Include(ci => ci.Shipment).ThenInclude(s => s.POFulfillment).Include(c => c.Shipment).ThenInclude(c => c.CargoDetails))
                 .ToListAsync();

            var shipments = consignmentItineraries.Select(x => x.Shipment).Distinct();

            if (shipments == null || !shipments.Any())
            {
                return;
            }

            try
            {
                // Handle for shipment is linked with PO by CargoDetails without booking module
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

                var poIdsToRelease = new List<long>();

                #region 7001 : VA - Vessel Departure / 7003 : VA - Flight Departure
                if (notification.Activity.ActivityCode == Event.EVENT_7001 || notification.Activity.ActivityCode == Event.EVENT_7003)
                {
                    //PO#1 - Shipment#1
                    //Shipment#1 has n FS
                    var shipmentIdsNotUpdate = consignmentItinerariesFSWithATD.Select(c => c.ShipmentId).ToList();
                    shipmentsLinkedPOWithoutBooking = shipmentsLinkedPOWithoutBooking.Where(c => !shipmentIdsNotUpdate.Any(s => s == c.Id)).ToList();
                    var ids = shipmentsLinkedPOWithoutBooking.Select(c => c.Id);
                    if (ids.Any() && poIdsWithoutBooking.Any())
                    {
                        _purchaseOrderService.AdjustQuantityOnPOLineItems(ids, poIdsWithoutBooking, AdjustBalanceOnPOLineItemsType.Return);
                    }

                    foreach (var poId in poIdsWithoutBooking)
                    {
                        var shipmentIdsLinkedPO = cargoDetails.Where(c => c.OrderId == poId).Select(c => c.ShipmentId).Distinct().ToList();
                        var isInvalid = consignmentItinerariesFSWithATD.Any(c => shipmentIdsLinkedPO.Contains(c.ShipmentId ?? 0));
                        if (isInvalid == false)
                        {
                            poIdsToRelease.Add(poId);
                        }
                    }

                    await _purchaseOrderService.RevertStageToReleasedAsync(poIdsToRelease);

                    List<long> poffIdListToRevert = new();
                    shipments = shipments.Where(c => c.POFulfillment != null).ToList();

                    foreach (var shipment in shipments)
                    {
                        if (freightScheduler.LocationFromName.Equals(shipment.ShipFrom ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                        {
                            if (shipment.POFulfillment.FulfilledFromPOType != POType.Blanket)
                            {
                                poffIdListToRevert.Add(shipment.POFulfillmentId.Value);
                            }
                            else
                            {
                                //TODO: handle for blanket booking
                            }
                        }
                    }
                    await _poFulfillmentService.RevertStageToFBConfirmedAsync(poffIdListToRevert, AppConstant.SYSTEM_USERNAME);
                    return;
                }
                #endregion

                #region 7002 : VA - Vessel Arrival / 7004 : VA - Flight Arrival
                if (notification.Activity.ActivityCode == Event.EVENT_7002 || notification.Activity.ActivityCode == Event.EVENT_7004)
                {
                    var consignmentItinerariesFSWithATA = await _consignmentItineraryRepository.QueryAsNoTracking(
                       c => shipmentsLinkedPO.Select(s => s.Id).ToList().Contains(c.ShipmentId ?? 0)
                           && c.Itinerary.FreightScheduler.ATADate != null, null,
                       c => c.Include(c => c.Itinerary).ThenInclude(c => c.FreightScheduler)
                       ).ToListAsync();

                    var poIdsToDispatch = new List<long>();

                    foreach (var poId in poIdsWithoutBooking)
                    {
                        var shipmentIdsLinkedPO = cargoDetails.Where(c => c.OrderId == poId).Select(c => c.ShipmentId).Distinct().ToList();
                        var isInvalid = consignmentItinerariesFSWithATA.Any(c => shipmentIdsLinkedPO.Contains(c.ShipmentId ?? 0));
                        if (isInvalid == false)
                        {
                            poIdsToDispatch.Add(poId);
                        }
                    }
                    await _purchaseOrderService.RevertStageToShipmentDispatchAsync(poIdsToDispatch);

                    if (freightScheduler.ATDDate == null && notification.IsDeletedViaFSApi.Value == true)
                    {
                        //PO#1 - Shipment#1
                        //Shipment#1 has n FS
                        var shipmentIdsNotUpdate = consignmentItinerariesFSWithATA.Select(c => c.ShipmentId).ToList();
                        shipmentsLinkedPOWithoutBooking = shipmentsLinkedPOWithoutBooking.Where(c => !shipmentIdsNotUpdate.Any(s => s == c.Id)).ToList();
                        var ids = shipmentsLinkedPOWithoutBooking.Select(c => c.Id);
                        if (ids.Any() && poIdsWithoutBooking.Any())
                        {
                            _purchaseOrderService.AdjustQuantityOnPOLineItems(ids, poIdsWithoutBooking, AdjustBalanceOnPOLineItemsType.Return);
                        }

                        foreach (var poId in poIdsWithoutBooking)
                        {
                            var shipmentIdsLinkedPO = cargoDetails.Where(c => c.OrderId == poId).Select(c => c.ShipmentId).Distinct().ToList();
                            var isHasNotAnyATD = consignmentItinerariesFSWithATD.Any(c => shipmentIdsLinkedPO.Contains(c.ShipmentId ?? 0));
                            var isHasNotAnyATA = consignmentItinerariesFSWithATA.Any(c => shipmentIdsLinkedPO.Contains(c.ShipmentId ?? 0));
                            if (isHasNotAnyATD == false && isHasNotAnyATA == false)
                            {
                                poIdsToRelease.Add(poId);
                            }
                        }

                        await _purchaseOrderService.RevertStageToReleasedAsync(poIdsToRelease);
                    }

                    List<long> poffIdListToRevert = new();
                    shipments = shipments.Where(c => c.POFulfillment != null).ToList();

                    foreach (var shipment in shipments)
                    {
                        if (shipment.ServiceType != null && (shipment.ServiceType.Contains("to-Port", StringComparison.OrdinalIgnoreCase) || shipment.ServiceType.Contains("to-Airport", StringComparison.OrdinalIgnoreCase))
                            && freightScheduler.LocationToName.Equals(shipment.ShipTo ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                            && shipment.Status.Equals(StatusType.ACTIVE, StringComparison.OrdinalIgnoreCase))
                        {
                            if (shipment.POFulfillment.FulfilledFromPOType != POType.Blanket)
                            {
                                poffIdListToRevert.Add(shipment.POFulfillmentId.Value);
                            }
                            else
                            {
                                //TODO: handle for blanket booking
                            }
                        }
                    }
                    await _poFulfillmentService.RevertStageToShipmentDispatchAsync(poffIdListToRevert, AppConstant.SYSTEM_USERNAME);
                    return;
                }
                #endregion
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}