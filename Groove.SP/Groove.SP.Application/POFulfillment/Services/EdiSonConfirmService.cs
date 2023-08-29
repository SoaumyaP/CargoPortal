using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

using EBooking = Groove.SP.Infrastructure.EBookingManagementAPI;
using Groove.SP.Application.POFulfillment.Services.Interfaces;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.Activity.Services.Interfaces;
using Groove.SP.Application.Interfaces.UnitOfWork;

namespace Groove.SP.Application.POFulfillment.Services
{
    public class EdiSonConfirmService : IEdiSonConfirmService
    {
        private readonly ICSFEApiClient _csfeApiClient;
        private readonly IRepository<POFulfillmentModel> _poffRepository;
        private readonly IRepository<ShipmentModel> _shipmentRepository;
        private readonly IActivityService _activityService;
        private readonly IPOFulfillmentService _poFulfillmentService;
        private readonly IUnitOfWork UnitOfWork;
       
        public EdiSonConfirmService(
            ICSFEApiClient csfeApiClient,
            IUnitOfWorkProvider unitOfWorkProvider,
            IRepository<POFulfillmentModel> poffRepository,
            IRepository<ShipmentModel> shipmentRepository,
            IActivityService activityService,
            IPOFulfillmentService poFulfillmentService)
        {
            UnitOfWork = unitOfWorkProvider.CreateUnitOfWork();
            _csfeApiClient = csfeApiClient;          
            _poffRepository = poffRepository;
            _shipmentRepository = shipmentRepository;
            _activityService = activityService;
            _poFulfillmentService = poFulfillmentService;
        }

        public async Task ConfirmPOFFAsync(EdiSonConfirmPOFFViewModel importVM)
        {
            var poFulfillment = await _poffRepository.GetAsync(
                x => x.BookingRequests.Any(br => br.BookingReferenceNumber == importVM.BookingReferenceNo
                && br.Status == POFulfillmentBookingRequestStatus.Active),
                null,
                i => i.Include(m => m.BookingRequests).Include(m => m.Itineraries));

            if (poFulfillment == null)
            {
                throw new AppEntityNotFoundException($"POFF with the BookingReferenceNo {importVM.BookingReferenceNo} not found!");
            }

            var bookingRequest = poFulfillment.BookingRequests.SingleOrDefault(x => x.Status == POFulfillmentBookingRequestStatus.Active);
            if (bookingRequest == null)
            {
                throw new AppValidationException($"Booking with the BookingReferenceNo {importVM.BookingReferenceNo} is inactive!");
            }

            if (poFulfillment.IsForwarderBookingItineraryReady || poFulfillment.Stage != POFulfillmentStage.ForwarderBookingRequest)
            {
                throw new AppValidationException("POFF is already confirmed!");
            }

            poFulfillment.IsForwarderBookingItineraryReady = true;
            poFulfillment.CYEmptyPickupTerminalCode = importVM.CYEmptyPickupTerminalCode;
            poFulfillment.CYEmptyPickupTerminalDescription = importVM.CYEmptyPickupTerminalDescription;
            poFulfillment.CFSWarehouseCode = importVM.CFSWarehouseCode;
            poFulfillment.CFSWarehouseDescription = importVM.CFSWarehouseDescription;
            poFulfillment.CYClosingDate = importVM.CYClosingDate;
            poFulfillment.CFSClosingDate = importVM.CFSClosingDate;

            bookingRequest.SONumber = importVM.SONumber;
            bookingRequest.BillOfLadingHeader = importVM.BillOfLadingHeader;
            bookingRequest.CYEmptyPickupTerminalCode = importVM.CYEmptyPickupTerminalCode;
            bookingRequest.CYEmptyPickupTerminalDescription = importVM.CYEmptyPickupTerminalDescription;
            bookingRequest.CFSWarehouseCode = importVM.CFSWarehouseCode;
            bookingRequest.CFSWarehouseDescription = importVM.CFSWarehouseDescription;
            bookingRequest.CYClosingDate = importVM.CYClosingDate;
            bookingRequest.CFSClosingDate = importVM.CFSClosingDate;

            var shipment = await _shipmentRepository.GetAsync(x => x.ShipmentNo == importVM.SONumber && x.Status == StatusType.ACTIVE);

            if (shipment != null)
            {
                shipment.POFulfillmentId = poFulfillment.Id;
            }

            foreach(var leg in importVM.Legs)
            {
                var loadingPort = await _csfeApiClient.GetLocationByCodeAsync(leg.LoadingPortCode);
                var dischargePort = await _csfeApiClient.GetLocationByCodeAsync(leg.DischargePortCode);
                var carrier = string.IsNullOrWhiteSpace(leg.CarrierCode) ? null :
                    await _csfeApiClient.GetCarrierByCodeAsync(leg.CarrierCode);

                var itinerary = new POFulfillmentItineraryModel
                {
                    Sequence = importVM.Legs.IndexOf(leg) + 1,
                    CreatedDate = DateTime.UtcNow,
                    ETADate = leg.ETA,
                    ETDDate = leg.ETD,
                    ModeOfTransport = leg.ModeOfTransport,
                    CarrierId = carrier?.Id,
                    CarrierName = carrier?.Name,
                    LoadingPortId = loadingPort?.Id ?? 0,
                    LoadingPort = loadingPort?.LocationDescription ?? leg.LoadingPortCode,
                    DischargePortId = dischargePort?.Id ?? 0,
                    DischargePort = dischargePort?.LocationDescription ?? leg.DischargePortCode,
                    VesselFlight = leg.VesselFlight,
                    Status = POFulfillmentItinerayStatus.Active
                };

                poFulfillment.Itineraries.Add(itinerary);
            }

            var event1052 = new ActivityViewModel()
            {
                ActivityCode = Event.EVENT_1052,
                POFulfillmentId = poFulfillment.Id,
                ActivityDate = DateTime.UtcNow,
                CreatedBy = AppConstant.SYSTEM_USERNAME
            };
            await _activityService.TriggerAnEvent(event1052);

            await _poFulfillmentService.ConfirmPurchaseOrderFulfillmentAsync(poFulfillment.Id, bookingRequest.SONumber, AppConstant.EDISON_USERNAME);
            await this.UnitOfWork.SaveChangesAsync();
        }
    }
}
