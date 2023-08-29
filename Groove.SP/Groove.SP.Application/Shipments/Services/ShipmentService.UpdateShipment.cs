using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.Container.ViewModels;
using Groove.SP.Application.Itinerary.ViewModels;
using Groove.SP.Application.ShipmentContact.Mappers;
using Groove.SP.Application.Shipments.ViewModels;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
namespace Groove.SP.Application.Shipments.Services;

public partial class ShipmentService
{
    protected readonly string[] ValidLogisticServiceTypeForAir =
    {
        LogisticServiceType.InternationalDoorToDoor,
        LogisticServiceType.InternationalAirportToAirport,
        LogisticServiceType.InternationalAirportToDoor,
        LogisticServiceType.InternationalDoorToAirport
    };
    protected readonly string[] ValidLogisticServiceTypeForSea =
    {
        LogisticServiceType.InternationalPortToPort,
        LogisticServiceType.InternationalPortToDoor,
        LogisticServiceType.InternationalDoorToPort,
        LogisticServiceType.InternationalDoorToDoor
    };
    public async Task<ImportingShipmentResultViewModel> UpdateFreightShipmentAsync(ImportShipmentViewModel importVM, string userName)
    {
        ImportingShipmentResultViewModel importingResult = new();
        List<Tuple<string, FreightSchedulerModel, char>> freightSchedulerEventList = new();
        List<FreightSchedulerModel> broadcastFreightScheduleIdList = new();
        var toCreateCargoDetails = new List<CargoDetailModel>();

        List<POFulfillmentContactModel> toCreatePOFulfillmentContacts = new();
        List<POFulfillmentContactModel> toDeletePOFulfillmentContacts = new();

        ItineraryModel firstItinerary = null;

        const char DELETE_EVENT = 'D';
        const char NEW_EVENT = 'N';
        const char UPDATE_EVENT = 'U';
        #region Shipment

        // Not include many navigation properties here as it depends on current payload to proceed
        // 1 shipment - 1 consignment
        var toUpdateShipment = await Repository.GetAsNoTrackingAsync(x => x.ShipmentNo == importVM.ShipmentNo && x.Status == StatusType.ACTIVE,
                        includes: x => x.Include(s => s.Consignments)
                                        .Include(s => s.Contacts)
                                        .Include(s => s.POFulfillment)
                        );

        // Check if shipment is inactive or not found
        if (toUpdateShipment is null)
        {
            importingResult.LogErrors($"{nameof(ImportShipmentViewModel.ShipmentNo)} is inactive or not existing in system.");
        }

        if (importVM.IsPropertyDirty(nameof(ImportShipmentViewModel.ShipFrom)))
        {
            var location = await _csfeApiClient.GetLocationByCodeAsync(importVM.ShipFrom);
            if (location is not null)
            {
                toUpdateShipment.ShipFrom = location.LocationDescription;
            }
            else
            {
                importingResult.LogErrors($"{nameof(ImportShipmentViewModel.ShipFrom)} is not existing in system.");
            }
        }

        if (importVM.IsPropertyDirty(nameof(ImportShipmentViewModel.ShipTo)))
        {
            var location = await _csfeApiClient.GetLocationByCodeAsync(importVM.ShipTo);
            if (location is not null)
            {
                toUpdateShipment.ShipTo = location.LocationDescription;
            }
            else
            {
                importingResult.LogErrors($"{nameof(ImportShipmentViewModel.ShipTo)} is not existing in system.");
            }
        }

        // Get linked booking
        if (importVM.IsPropertyDirty(nameof(importVM.BookingReferenceNo)))
        {
            var bookingRequest = await _poFulfillmentBookingRequestRepository.QueryAsNoTracking(
                    x => x.BookingReferenceNumber == importVM.BookingReferenceNo, null, x => x.Include(y => y.POFulfillment)
                ).FirstOrDefaultAsync(x => x.Status == POFulfillmentBookingRequestStatus.Active);

            if (bookingRequest is null)
            {
                importingResult.LogErrors($"{nameof(ImportShipmentViewModel.BookingReferenceNo)} is not existing in system.");
            }
            else
            {
                var poFulfillment = bookingRequest.POFulfillment;
                if (poFulfillment is not null)
                {
                    toUpdateShipment.CargoReadyDate = poFulfillment.CargoReadyDate ?? default;
                    toUpdateShipment.BookingDate = poFulfillment.BookingDate ?? default;
                    toUpdateShipment.BookingNo = poFulfillment.Number;
                    toUpdateShipment.POFulfillmentId = poFulfillment.Id;
                    toUpdateShipment.POFulfillment = poFulfillment;
                }
            }
        }

        // Quick return if some failed
        if (!importingResult.Success)
        {
            return importingResult;
        }

        // Update shipment headers
        if (importVM.IsPropertyDirty(nameof(importVM.ModeOfTransport)))
        {
            toUpdateShipment.ModeOfTransport = importVM.ModeOfTransport;
        }
        var isAirShipment = toUpdateShipment.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase);

        if (importVM.IsPropertyDirty(nameof(importVM.BuyerCode)))
        {
            toUpdateShipment.BuyerCode = importVM.BuyerCode;
        }
        if (importVM.IsPropertyDirty(nameof(importVM.CustomerReferenceNo)))
        {
            toUpdateShipment.CustomerReferenceNo = importVM.CustomerReferenceNo;
        }
        if (importVM.IsPropertyDirty(nameof(importVM.AgentReferenceNo)))
        {
            toUpdateShipment.AgentReferenceNo = importVM.AgentReferenceNo;
        }
        if (importVM.IsPropertyDirty(nameof(importVM.ShipperReferenceNo)))
        {
            toUpdateShipment.ShipperReferenceNo = importVM.ShipperReferenceNo;
        }
        if (importVM.IsPropertyDirty(nameof(importVM.CarrierContractNo)))
        {
            toUpdateShipment.CarrierContractNo = importVM.CarrierContractNo;
        }
        if (importVM.IsPropertyDirty(nameof(importVM.Movement)))
        {
            toUpdateShipment.Movement = importVM.Movement;

            if (!isAirShipment)
            {
                if (!string.IsNullOrEmpty(importVM.Movement))
                {
                    toUpdateShipment.IsFCL = importVM.Movement.StartsWith("CY", StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    importingResult.LogErrors($"{nameof(importVM.Movement)} must not be empty.");
                }
            }
        }
        if (importVM.IsPropertyDirty(nameof(importVM.ServiceType)))
        {
            var validServiceType = isAirShipment ? ValidLogisticServiceTypeForAir : ValidLogisticServiceTypeForSea;

            if (validServiceType.Contains(importVM.ServiceType, StringComparer.InvariantCultureIgnoreCase))
            {
                toUpdateShipment.ServiceType = importVM.ServiceType;
            }
            else
            {
                importingResult.LogErrors($"{nameof(ImportShipmentViewModel.ServiceType)} must be '{string.Join(", ", validServiceType)}'.");
            }
        }
        if (importVM.IsPropertyDirty(nameof(importVM.Incoterm)))
        {
            toUpdateShipment.Incoterm = importVM.Incoterm;
        }
        if (importVM.IsPropertyDirty(nameof(importVM.Factor)))
        {
            toUpdateShipment.Factor = importVM.Factor;
        }
        if (importVM.IsPropertyDirty(nameof(importVM.ShipFromETDDate)))
        {
            toUpdateShipment.ShipFromETDDate = importVM.ShipFromETDDate;
        }
        if (importVM.IsPropertyDirty(nameof(importVM.ShipToETADate)))
        {
            toUpdateShipment.ShipToETADate = importVM.ShipToETADate;
        }
        toUpdateShipment.UpdatedBy = userName;
        toUpdateShipment.UpdatedDate = DateTime.UtcNow;

        CalculateShipmentTotal(importVM, ref toUpdateShipment);

        #endregion Shipment

        #region Consignments

        var toCreateConsignments = new List<ConsignmentModel>();
        var toUpdateConsignments = new List<ConsignmentModel>();

        // There is only one Consignment
        var availableConsignment = toUpdateShipment.Consignments.FirstOrDefault();

        if (availableConsignment == null)
        {
            availableConsignment = new ConsignmentModel
            {
                Sequence = 1,
                TriangleTradeFlag = false,
                MemoBOLFlag = false,
                ConsignmentType = OrganizationRole.OriginAgent,
                ShipmentLoads = new List<ShipmentLoadModel>(),
                ShipmentLoadDetails = new List<ShipmentLoadDetailModel>()
            };
        }
        availableConsignment.PopulateFromShipment(toUpdateShipment);
        availableConsignment.Audit(userName);

        if (availableConsignment.Id == 0)
        {
            toCreateConsignments.Add(availableConsignment);
        }
        else
        {
            toUpdateConsignments.Add(availableConsignment);
        }

        #endregion Consignments

        #region ShipmentContacts
        var toDeleteShipmentContacts = new List<ShipmentContactModel>();
        var toCreateShipmentContacts = new List<ShipmentContactModel>();

        if (importVM.Contacts != null && importVM.Contacts.Any())
        {
            // Remove all existing data
            toDeleteShipmentContacts.AddRange(toUpdateShipment.Contacts);

            toUpdateShipment.Contacts = new List<ShipmentContactModel>();

            // Then create new list
            await MappingContactAsync(importVM, toUpdateShipment, importingResult);
            toCreateShipmentContacts = toUpdateShipment.Contacts.ToList();
            foreach (var item in toCreateShipmentContacts)
            {
                item.ShipmentId = toUpdateShipment.Id;
                item.Shipment = toUpdateShipment;
            }
        }
        // Quick return if there are some errors on mapping contacts 
        if (!importingResult.Success)
        {
            return importingResult;
        }

        // If bulk booking, copy destination agent contact
        if (toUpdateShipment.POFulfillment != null && toUpdateShipment.POFulfillment.FulfillmentType == FulfillmentType.Bulk)
        {
            if (toUpdateShipment.Contacts != null && toUpdateShipment.Contacts.Any())
            {
                var destinationAgent = toUpdateShipment.Contacts?.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.DestinationAgent, StringComparison.InvariantCultureIgnoreCase));

                if (destinationAgent != null)
                {
                    // Remove current destination agent before creating a new one

                    var storedDestinationAgent = await _poFulfillmentContactRepository.QueryAsNoTracking(
                        c => c.POFulfillmentId == toUpdateShipment.POFulfillment.Id && c.OrganizationRole == destinationAgent.OrganizationRole).ToListAsync();
                    toDeletePOFulfillmentContacts.AddRange(storedDestinationAgent);

                    toCreatePOFulfillmentContacts.Add(new()
                    {
                        POFulfillmentId = toUpdateShipment.POFulfillment.Id,
                        OrganizationId = destinationAgent.OrganizationId,
                        OrganizationRole = destinationAgent.OrganizationRole,
                        CompanyName = destinationAgent.CompanyName,
                        ContactName = destinationAgent.ContactName,
                        ContactEmail = destinationAgent.ContactEmail,
                        ContactNumber = destinationAgent.ContactNumber,
                        Address = CompanyAddressLinesResolver.SplitCompanyAddressLines(destinationAgent.Address, 1),
                        AddressLine2 = CompanyAddressLinesResolver.SplitCompanyAddressLines(destinationAgent.Address, 2),
                        AddressLine3 = CompanyAddressLinesResolver.SplitCompanyAddressLines(destinationAgent.Address, 3),
                        AddressLine4 = CompanyAddressLinesResolver.SplitCompanyAddressLines(destinationAgent.Address, 4),
                        CreatedBy = userName,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedBy = userName,
                        UpdatedDate = DateTime.UtcNow
                    });
                }
            }
        }
        #endregion ShipmentContacts

        #region Itineraries
        // Master bill of ladings linked to shipment
        var availableMasterBillOfLadings = await _masterBillOfLadingRepository.Query(x => x.Consignments.Any(y => y.ShipmentId == toUpdateShipment.Id), includes: x => x.Include(y => y.Contacts)).ToListAsync();
        string availableMasterBillNo = availableMasterBillOfLadings.FirstOrDefault()?.MasterBillOfLadingNo;

        var isOldMasterBillNo = true;
        if (importVM.IsPropertyDirty(nameof(importVM.MasterNo)))
        {
            isOldMasterBillNo = availableMasterBillNo != null && importVM.MasterNo == availableMasterBillNo;
            availableMasterBillNo = importVM.MasterNo;
        }

        var toCreateItineraries = new List<ItineraryModel>();
        var toDeleteItineraries = new List<ItineraryModel>();
        var toUpdateItineraries = new List<ItineraryModel>();

        var toCreateConsignmentItineraries = new List<ConsignmentItineraryModel>();
        var toDeleteConsignmentItineraries = new List<ConsignmentItineraryModel>();

        var toDeteleBillOfLadingItineraries = new List<BillOfLadingItineraryModel>();

        // List of available itineraries, to link to Bill of lading
        var availableItineraries = await _itineraryRepository.Query(i => i.ConsignmentItineraries.Any(ci => ci.ShipmentId == toUpdateShipment.Id),
                                                                        includes: x => x.Include(y => y.ConsignmentItineraries)
                                                                                        .Include(y => y.ContainerItineraries)
                                                                                        .Include(y => y.BillOfLadingItineraries)
                                                                                        .Include(y => y.MasterBillOfLadingItineraries)
                                                                                        .Include(y => y.FreightScheduler)
                                                                   ).ToListAsync();

        var forceToUpdateItinerary = false;
        if (!isOldMasterBillNo && (importVM.Itineraries is null || !importVM.Itineraries.Any()))
        {
            if (availableItineraries != null && availableItineraries.Any(x => x.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase)))
            {
                importVM.Itineraries = new List<ImportItineraryViewModel>();
                foreach (var item in availableItineraries)
                {
                    var isAirItinerary = item.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase);
                    var itineraryVM = new ImportItineraryViewModel
                    {
                        ModeOfTransport = item.ModeOfTransport,
                        Sequence = item.Sequence,
                        CarrierCode = isAirItinerary ? item.AirlineCode : item.SCAC,
                        VesselName = item.VesselName,
                        Voyage = item.Voyage,
                        FlightNumber = item.FlightNumber,
                        LoadingPort = item.LoadingPort,
                        DischargePort = item.DischargePort,
                        ETDDate = item.ETDDate,
                        ETADate = item.ETADate,
                        CYClosingDate = item.CYClosingDate,
                        CYOpenDate = item.CYOpenDate,
                        FieldStatus = new()
                    };
                    itineraryVM.FieldStatus[nameof(itineraryVM.ETDDate)] = FieldDeserializationStatus.HasValue;
                    itineraryVM.FieldStatus[nameof(itineraryVM.ETADate)] = FieldDeserializationStatus.HasValue;
                    if (item.FreightScheduler != null)
                    {
                        if (item.FreightScheduler.ATDDate.HasValue)
                        {
                            itineraryVM.ATDDate = item.FreightScheduler.ATDDate;
                            itineraryVM.FieldStatus[nameof(itineraryVM.ATDDate)] = FieldDeserializationStatus.HasValue;
                        }
                        if (item.FreightScheduler.ATADate.HasValue)
                        {
                            itineraryVM.ATADate = item.FreightScheduler.ATADate;
                            itineraryVM.FieldStatus[nameof(itineraryVM.ATADate)] = FieldDeserializationStatus.HasValue;
                        }
                    }
                    importVM.Itineraries.Add(itineraryVM);
                }

                forceToUpdateItinerary = true;
            }
        }

        // Itineraries in payload
        if (importVM.Itineraries is not null && importVM.Itineraries.Any())
        {
            var toUnlinkItineraries = availableItineraries;
            var toUnlinkItineraryIds = toUnlinkItineraries.Select(x => x.Id).ToList();
            var itineraryShipmentLinks = await _consignmentItineraryRepository.Query(ci => toUnlinkItineraryIds.Contains(ci.ItineraryId)).Select(x => new { x.ItineraryId, x.ShipmentId }).Distinct().ToListAsync();

            // Itineraries not sharing with other shipments
            toDeleteItineraries = toUnlinkItineraries.Where(x => itineraryShipmentLinks.Count(y => y.ItineraryId == x.Id) == 1).ToList();
            toDeteleBillOfLadingItineraries = toDeleteItineraries.SelectMany(x => x.BillOfLadingItineraries).ToList();

            // Itineraries sharing with other shipments
            toUnlinkItineraries = toUnlinkItineraries.Where(x => itineraryShipmentLinks.Count(y => y.ItineraryId == x.Id) > 1).ToList();
            foreach (var item in toUnlinkItineraries)
            {
                // Remove appropriate consignment itinerary
                var toRemoveItem = item.ConsignmentItineraries.Where(x => x.ShipmentId == toUpdateShipment.Id).ToList();
                toDeleteConsignmentItineraries.AddRange(toRemoveItem);
            }

            // Reset Itinearies
            availableItineraries = new();
            foreach (var (itineraryVM, index) in importVM.Itineraries.Select((item, index) => (item, index)))
            {
                ItineraryModel newItinerary = new();
                Mapper.Map(itineraryVM, newItinerary);
                newItinerary.Status = StatusType.ACTIVE;
                newItinerary.Audit(userName);

                // Link shipment & consignment via ConsignmentItinerary
                var newConsignmentItinerary = new ConsignmentItineraryModel
                {
                    Shipment = toUpdateShipment,
                    ShipmentId = toUpdateShipment.Id,
                    Consignment = availableConsignment,
                    ConsignmentId = availableConsignment.Id,
                    CreatedBy = userName,
                    CreatedDate = DateTime.Now,
                    UpdatedBy = userName,
                    UpdatedDate = DateTime.Now,
                };
                newItinerary.ConsignmentItineraries = new List<ConsignmentItineraryModel>
                        {
                            newConsignmentItinerary
                        };

                if (!forceToUpdateItinerary)
                {
                    var loadingPortLocation = await _csfeApiClient.GetLocationByCodeAsync(itineraryVM.LoadingPort);
                    if (loadingPortLocation != null)
                    {
                        newItinerary.LoadingPort = loadingPortLocation.LocationDescription;
                    }
                    else
                    {
                        importingResult.LogErrors($"{nameof(ImportShipmentViewModel.Itineraries)}[{index}].{nameof(ImportItineraryViewModel.LoadingPort)} is not existing in system.");
                    }

                    var dischargePortLocation = await _csfeApiClient.GetLocationByCodeAsync(itineraryVM.DischargePort);
                    if (dischargePortLocation != null)
                    {
                        newItinerary.DischargePort = dischargePortLocation.LocationDescription;
                    }
                    else
                    {
                        importingResult.LogErrors($"{nameof(ImportShipmentViewModel.Itineraries)}[{index}].{nameof(ImportItineraryViewModel.DischargePort)} is not existing in system.");
                    }
                }

                var carrier = await _csfeApiClient.GetCarrierByCodeAsync(itineraryVM.CarrierCode);
                if (carrier != null)
                {
                    newItinerary.CarrierName = carrier.Name;
                }

                var isAirItinerary = newItinerary.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase);
                var isSeaOrAir = newItinerary.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase) || isAirItinerary;
                // Not sea/air, create immediately
                if (!isSeaOrAir)
                {
                    toCreateItineraries.Add(newItinerary);
                    // Set for futher uses
                    availableItineraries.Add(newItinerary);
                    continue;
                }

                var departureEventCode = isAirItinerary ? Event.EVENT_7003 : Event.EVENT_7001;
                var arrivalEventCode = isAirItinerary ? Event.EVENT_7004 : Event.EVENT_7002;

                Expression<Func<FreightSchedulerModel, bool>> freightSchedulerFilter = null;

                if (isAirItinerary)
                {
                    if (string.IsNullOrEmpty(availableMasterBillNo))
                    {
                        importingResult.LogErrors($"{nameof(ImportShipmentViewModel.Itineraries)}[{index}]#Master Bill Number is required when importing Air Itinerary.");
                    }

                    freightSchedulerFilter = fs =>
                        fs.ModeOfTransport == newItinerary.ModeOfTransport
                        && fs.MAWB == availableMasterBillNo;
                }
                else
                {
                    freightSchedulerFilter = fs =>
                        fs.ModeOfTransport == newItinerary.ModeOfTransport &&
                        fs.CarrierCode == itineraryVM.CarrierCode &&
                        fs.VesselName == newItinerary.VesselName &&
                        fs.Voyage == newItinerary.Voyage &&
                        fs.LocationFromName == newItinerary.LoadingPort &&
                        fs.LocationToName == newItinerary.DischargePort;
                }

                // Check if FrieghtScheduler record found with the same value of ModeofTransport = SEA, SCAC, VesselName, Voyage, LoadingPort, DischargePort
                var existingFreightScheduler = await _freightSchedulerRepository.GetAsync(freightSchedulerFilter,
                                                                    includes: i => i.Include(y => y.Itineraries).ThenInclude(y => y.ConsignmentItineraries));

                // If not found, create a new FreightScheduler
                if (existingFreightScheduler == null)
                {
                    var newFreightScheduler = new FreightSchedulerModel
                    {
                        ModeOfTransport = newItinerary.ModeOfTransport,
                        CarrierCode = itineraryVM.CarrierCode,
                        CarrierName = newItinerary.CarrierName,
                        VesselName = newItinerary.VesselName,
                        Voyage = newItinerary.Voyage,
                        FlightNumber = newItinerary.FlightNumber,
                        MAWB = isAirItinerary ? availableMasterBillNo : null,
                        LocationFromCode = itineraryVM.LoadingPort,
                        LocationFromName = newItinerary.LoadingPort,
                        LocationToCode = itineraryVM.DischargePort,
                        LocationToName = newItinerary.DischargePort,
                        ETDDate = newItinerary.ETDDate,
                        ETADate = newItinerary.ETADate,
                        ATADate = itineraryVM.ATADate.HasValue ? itineraryVM.ATADate : null,
                        ATDDate = itineraryVM.ATDDate.HasValue ? itineraryVM.ATDDate : null,
                        CYClosingDate = itineraryVM.CYClosingDate.HasValue ? itineraryVM.CYClosingDate : null,
                        CYOpenDate = itineraryVM.CYOpenDate.HasValue ? itineraryVM.CYOpenDate : null,
                        IsAllowExternalUpdate = true
                    };
                    newFreightScheduler.Audit(userName);
                    newItinerary.FreightScheduler = newFreightScheduler;
                    broadcastFreightScheduleIdList.Add(newFreightScheduler);

                    if (itineraryVM.IsPropertyDirty(nameof(ImportItineraryViewModel.ATDDate)) && itineraryVM.ATDDate.HasValue)
                    {
                        freightSchedulerEventList.Add(new(departureEventCode, newFreightScheduler, NEW_EVENT));
                    }

                    if (itineraryVM.IsPropertyDirty(nameof(ImportItineraryViewModel.ATADate)) && itineraryVM.ATADate.HasValue)
                    {
                        freightSchedulerEventList.Add(new(arrivalEventCode, newFreightScheduler, NEW_EVENT));
                    }

                    // Create Itinerary will auto create FS
                    toCreateItineraries.Add(newItinerary);

                    // Set for further uses
                    availableItineraries.Add(newItinerary);
                }
                else
                {
                    if (existingFreightScheduler.IsAllowExternalUpdate)
                    {
                        var currentATD = existingFreightScheduler.ATDDate;
                        var currentATA = existingFreightScheduler.ATADate;

                        var currentETD = existingFreightScheduler.ETDDate;
                        var currentETA = existingFreightScheduler.ETADate;

                        // Update Freight Scheduler dates

                        // check to allow update ATD
                        if (itineraryVM.IsPropertyDirty(nameof(ImportItineraryViewModel.ATDDate)))
                        {
                            var checkReadyContainerManifestResult = await _freightSchedulerService.IsReadyContainerManifestAsync(existingFreightScheduler.Id);

                            var canUpdateATD = checkReadyContainerManifestResult.Item1 || (
                                    // Due to current shipment only
                                    checkReadyContainerManifestResult.Item2.Count == 1
                                    && checkReadyContainerManifestResult.Item2.Contains(toUpdateShipment.Id)
                                    && importVM.Containers != null && importVM.Containers.Any());

                            if (!canUpdateATD && !existingFreightScheduler.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase))
                            {
                                importingResult.LogErrors($"{nameof(ImportShipmentViewModel.Itineraries)}[{index}].{nameof(ImportItineraryViewModel.ATDDate)} can not update because the container manifest not ready for all shipments.");
                                return importingResult;
                            }
                            else
                            {
                                existingFreightScheduler.ATDDate = itineraryVM.ATDDate;
                            }
                        }

                        if (itineraryVM.IsPropertyDirty(nameof(ImportItineraryViewModel.ETDDate)))
                        {
                            existingFreightScheduler.ETDDate = itineraryVM.ETDDate;
                        }
                        if (itineraryVM.IsPropertyDirty(nameof(ImportItineraryViewModel.ETADate)))
                        {
                            existingFreightScheduler.ETADate = itineraryVM.ETADate;
                        }
                        if (itineraryVM.IsPropertyDirty(nameof(ImportItineraryViewModel.ATADate)))
                        {
                            existingFreightScheduler.ATADate = itineraryVM.ATADate;
                        }
                        if (itineraryVM.IsPropertyDirty(nameof(ImportItineraryViewModel.CYOpenDate)))
                        {
                            existingFreightScheduler.CYOpenDate = itineraryVM.CYOpenDate;
                        }
                        if (itineraryVM.IsPropertyDirty(nameof(ImportItineraryViewModel.CYClosingDate)))
                        {
                            existingFreightScheduler.CYClosingDate = itineraryVM.CYClosingDate;
                        }

                        // add new events
                        if (!currentATD.HasValue && existingFreightScheduler.ATDDate.HasValue)
                        {
                            freightSchedulerEventList.Add(new(departureEventCode, existingFreightScheduler, NEW_EVENT));
                        }
                        if (!currentATA.HasValue && existingFreightScheduler.ATADate.HasValue)
                        {
                            freightSchedulerEventList.Add(new(arrivalEventCode, existingFreightScheduler, NEW_EVENT));
                        }

                        // update events
                        if (currentATD.HasValue && existingFreightScheduler.ATDDate.HasValue && currentATD.Value != existingFreightScheduler.ATDDate.Value)
                        {
                            freightSchedulerEventList.Add(new(departureEventCode, existingFreightScheduler, UPDATE_EVENT));
                        }
                        if (currentATA.HasValue && existingFreightScheduler.ATADate.HasValue && currentATA.Value != existingFreightScheduler.ATADate.Value)
                        {
                            freightSchedulerEventList.Add(new(arrivalEventCode, existingFreightScheduler, UPDATE_EVENT));
                        }

                        // delete events
                        if (currentATD.HasValue && !existingFreightScheduler.ATDDate.HasValue)
                        {
                            freightSchedulerEventList.Add(new(departureEventCode, existingFreightScheduler, DELETE_EVENT));
                        }
                        if (currentATA.HasValue && !existingFreightScheduler.ATADate.HasValue)
                        {
                            freightSchedulerEventList.Add(new(arrivalEventCode, existingFreightScheduler, DELETE_EVENT));
                        }

                        if (currentETD != existingFreightScheduler.ETDDate || currentETA != existingFreightScheduler.ETADate)
                        {
                            broadcastFreightScheduleIdList.Add(existingFreightScheduler);
                        }
                    }

                    // To update existing FreightScheduler
                    if (existingFreightScheduler.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (itineraryVM.IsPropertyDirty(nameof(ImportItineraryViewModel.CarrierCode)))
                        {
                            existingFreightScheduler.CarrierCode = newItinerary.AirlineCode;
                            existingFreightScheduler.CarrierName = newItinerary.CarrierName;
                        }
                        if (itineraryVM.IsPropertyDirty(nameof(ImportItineraryViewModel.FlightNumber)))
                        {
                            existingFreightScheduler.FlightNumber = newItinerary.FlightNumber;
                        }
                        if (itineraryVM.IsPropertyDirty(nameof(ImportItineraryViewModel.LoadingPort)))
                        {
                            existingFreightScheduler.LocationFromName = newItinerary.LoadingPort;
                        }
                        if (itineraryVM.IsPropertyDirty(nameof(ImportItineraryViewModel.DischargePort)))
                        {
                            existingFreightScheduler.LocationToName = newItinerary.DischargePort;
                        }
                    }

                    // Check if any Itinerary has the same ScheduleId & Sequence
                    var existingItinerary = existingFreightScheduler.Itineraries?.FirstOrDefault(x => x.Sequence == newItinerary.Sequence);

                    if (existingItinerary != null)
                    {
                        // To update existing Itinerary
                        if (itineraryVM.IsPropertyDirty(nameof(ImportItineraryViewModel.ETDDate)))
                        {
                            existingItinerary.ETDDate = itineraryVM.ETDDate;
                        }
                        if (itineraryVM.IsPropertyDirty(nameof(ImportItineraryViewModel.ETADate)))
                        {
                            existingItinerary.ETADate = itineraryVM.ETADate;
                        }
                        if (itineraryVM.IsPropertyDirty(nameof(ImportItineraryViewModel.CYOpenDate)))
                        {
                            existingItinerary.CYOpenDate = itineraryVM.CYOpenDate;
                        }
                        if (itineraryVM.IsPropertyDirty(nameof(ImportItineraryViewModel.CYClosingDate)))
                        {
                            existingItinerary.CYClosingDate = itineraryVM.CYClosingDate;
                        }
                        if (existingItinerary.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (itineraryVM.IsPropertyDirty(nameof(ImportItineraryViewModel.CarrierCode)))
                            {
                                existingItinerary.AirlineCode = newItinerary.AirlineCode;
                                existingItinerary.CarrierName = newItinerary.CarrierName;
                            }
                            if (itineraryVM.IsPropertyDirty(nameof(ImportItineraryViewModel.FlightNumber)))
                            {
                                existingItinerary.FlightNumber = newItinerary.FlightNumber;
                                existingItinerary.VesselFlight = newItinerary.FlightNumber;
                            }
                            if (itineraryVM.IsPropertyDirty(nameof(ImportItineraryViewModel.LoadingPort)))
                            {
                                existingItinerary.LoadingPort = newItinerary.LoadingPort;
                            }
                            if (itineraryVM.IsPropertyDirty(nameof(ImportItineraryViewModel.DischargePort)))
                            {
                                existingItinerary.DischargePort = newItinerary.DischargePort;
                            }
                        }

                        // If found the existing, update information
                        // Add new ConsignmentItinerary

                        newConsignmentItinerary.ItineraryId = existingItinerary.Id;
                        var isExisting = existingItinerary.ConsignmentItineraries.Any(a => a.ConsignmentId == newConsignmentItinerary.ConsignmentId);
                        if (!isExisting)
                        {
                            toCreateConsignmentItineraries.Add(newConsignmentItinerary);
                        }

                        toUpdateItineraries.Add(existingItinerary);

                        // Set value for futher uses
                        availableItineraries.Add(existingItinerary);
                    }
                    else
                    {
                        // If not found, add new Itinerary
                        // some related/navigation properties will be created auto also
                        newItinerary.FreightScheduler = existingFreightScheduler;
                        toCreateItineraries.Add(newItinerary);
                        // Set value for futher uses
                        availableItineraries.Add(newItinerary);
                    }
                }
            }

            toDeleteItineraries = toDeleteItineraries.Except(availableItineraries).ToList();

            toDeleteConsignmentItineraries = toDeleteConsignmentItineraries.Except(
                availableItineraries.SelectMany(x => x.ConsignmentItineraries).Where(x => x.ConsignmentId == availableConsignment.Id)).ToList();
        }

        // Quick return if some meta data of Itinerary unavailable in Master data
        if (!importingResult.Success)
        {
            return importingResult;
        }

        #endregion Itineraries

        #region Containers/ShipmentLoads/Consolidations

        var containerNumbers = importVM.Containers?.Select(x => x.ContainerNo).ToList() ?? new();
        var toCreateContainers = new List<ContainerModel>();
        var toUpdateCFSContainers = new List<ContainerModel>();
        var toUpdateConsolidations = new List<ConsolidationModel>();
        var toDeleteContainers = new List<ContainerModel>();
        var toDeleteConsolidations = new List<ConsolidationModel>();

        var toDeleteShipmentLoads = new List<ShipmentLoadModel>();
        var toDeleteBillOfLadingShipmentLoads = new List<BillOfLadingShipmentLoadModel>();
        var toUpdateBillOfLadingShipmentLoads = new List<BillOfLadingShipmentLoadModel>();

        var toDeleteShipmentLoadDetails = new List<ShipmentLoadDetailModel>();
        var toDeleteContainerItineraries = new List<ContainerItineraryModel>();

        // List of available containers, to link to ShipmentLoadDetails
        var availableContainers = new List<ContainerModel>();

        // To set some values for container
        firstItinerary = availableItineraries.FirstOrDefault();

        var toCreateShipmentLoads = new List<ShipmentLoadModel>();

        // List of available ShipmentLoads, to link to ShipmentLoadDetails
        var availableShipmentLoads = new List<ShipmentLoadModel>();

        // For CFS container
        var toCreateConsolidations = new List<ConsolidationModel>();

        // Get containers stored in database = CY + CFS
        if (!isAirShipment) // there is no Container for AIR
        {
            availableContainers = await _containerRepository.Query(x => x.ShipmentLoads.Any(x => x.ShipmentId == toUpdateShipment.Id),
                                                                    includes: x => x.Include(y => y.Consolidation)
                                                                                    .Include(y => y.ContainerItineraries)
                                                                                    .Include(y => y.ShipmentLoads).ThenInclude(y => y.BillOfLadingShipmentLoads)
                                                                                    .Include(y => y.ShipmentLoads).Include(y => y.ShipmentLoadDetails)
                                                             ).ToListAsync();
        }
        else
        {
            // for AIR shipment. there is only one default ShipmentLoad. 
            availableShipmentLoads = await _shipmentLoadRepository.Query(x => x.ShipmentId == toUpdateShipment.Id,
                                                                    includes: x => x.Include(y => y.BillOfLadingShipmentLoads)
                                                                                    .Include(y => y.ShipmentLoadDetails)
                                                             ).ToListAsync();
        }
        

        // Get ShipmentLoads stored in database
        if (availableContainers != null && availableContainers.Any())
        {
            availableShipmentLoads = availableContainers?.SelectMany(x => x.ShipmentLoads).Where(x => x.ShipmentId == toUpdateShipment.Id).ToList();
        }

        // Any container in payload
        if (containerNumbers.Any() && !isAirShipment) // as business rule, there is no Container for AIR
        {
            // Need to check CFS containers in sharing
            var cfsContainers = availableContainers.Where(x => !x.IsFCL).ToList();
            var cfsContainerIds = cfsContainers.Select(x => x.Id).ToList();

            // Links between container-shipment
            var containerShipmentLinks = await _shipmentLoadRepository.Query(sl => sl.ContainerId != null && cfsContainerIds.Contains(sl.ContainerId.Value)).Select(x => new { x.ContainerId, x.ShipmentId }).Distinct().ToListAsync();

            // Remove CY containers
            toDeleteContainers = availableContainers.Where(x => x.IsFCL).ToList();

            // Set to empty
            availableContainers = new();

            // CFS Containers not sharing with other shipments
            toDeleteContainers.AddRange(cfsContainers.Where(x => containerShipmentLinks.Count(y => y.ContainerId == x.Id) == 1).ToList());
            // Delete Consolidations
            toDeleteConsolidations = toDeleteContainers.Where(x => x.Consolidation != null).Select(x => x.Consolidation).ToList();

            var toDeleteContainerIds = toDeleteContainers.Select(x => x.Id).ToList();

            // Reset containers -> remove all related ShipmentLoads/ShipmentLoadDetails/BillOfLadingShipmentsLoads    
            // Remove all stored ShipmentLoads
            // Removing ShipmentLoads will remove ShipmentLoadDetails also
            toDeleteShipmentLoads = toDeleteContainers.SelectMany(x => x.ShipmentLoads).ToList();

            toDeleteShipmentLoads.AddRange(availableShipmentLoads);
            toDeleteShipmentLoads = toDeleteShipmentLoads.Distinct().ToList();
            // Set to empty
            availableShipmentLoads = new();

            // Remove all stored ShipmentLoadDetail
            toDeleteShipmentLoadDetails.AddRange(toDeleteShipmentLoads.Where(x => x.ShipmentLoadDetails != null).SelectMany(x => x.ShipmentLoadDetails).ToList());

            // Remove all stored ContainerItineraries
            toDeleteContainerItineraries = toDeleteContainers.SelectMany(x => x.ContainerItineraries).ToList();

            // CY Containers -> Remove BillOfLadingShipmentLoads
            toDeleteBillOfLadingShipmentLoads = toDeleteContainers.SelectMany(x => x.ShipmentLoads)?.Where(x => x.BillOfLadingShipmentLoads != null).SelectMany(x => x.BillOfLadingShipmentLoads).ToList();

            // CFS container -> BillOfLadingShipmentLoads: remove ContainerId
            var inSharingCFSContainers = cfsContainers.Where(x => containerShipmentLinks.Count(y => y.ContainerId == x.Id) > 1).ToList();
            toUpdateBillOfLadingShipmentLoads = inSharingCFSContainers.SelectMany(x => x.ShipmentLoads)?
                .Where(x => x.BillOfLadingShipmentLoads != null)
                .Except(toDeleteShipmentLoads)
                .SelectMany(x => x.BillOfLadingShipmentLoads)
                .ToList();
            foreach (var item in toUpdateBillOfLadingShipmentLoads)
            {
                item.ContainerId = null;
                item.Container = null;
            }
            // Re-calculate Weight, Volume, Unit, Package of the unlink CFS Container / Consolidation
            foreach (var item in inSharingCFSContainers)
            {
                item.ShipmentLoadDetails = item.ShipmentLoadDetails?.Where(x => x.ShipmentId != toUpdateShipment.Id).ToList() ?? new();

                item.TotalGrossWeight = item.ShipmentLoadDetails.Sum(x => x.GrossWeight);
                item.TotalNetWeight = item.ShipmentLoadDetails.Sum(x => x.NetWeight ?? 0);
                item.TotalVolume = item.ShipmentLoadDetails.Sum(x => x.Volume);
                item.TotalPackage = (int)item.ShipmentLoadDetails.Sum(x => x.Package);
                var consolidation = item.Consolidation;
                if (consolidation != null)
                {
                    consolidation.TotalGrossWeight = item.TotalGrossWeight;
                    consolidation.TotalNetWeight = item.TotalNetWeight;
                    consolidation.TotalVolume = item.TotalVolume;
                    consolidation.TotalPackage = item.TotalPackage;
                }
            }

            toUpdateCFSContainers.AddRange(inSharingCFSContainers);
            toUpdateConsolidations.AddRange(toUpdateCFSContainers.Select(x => x.Consolidation));

            // Loop in payload then create new containers
            foreach (var containerVM in importVM.Containers)
            {
                var newContainer = new ContainerModel
                {
                    ContainerNo = containerVM.ContainerNo,
                    ContainerType = EnumExtension.GetEnumMemberValue<EquipmentType>(containerVM.ContainerType),
                    LoadingDate = containerVM.LoadingDate.HasValue ? containerVM.LoadingDate : firstItinerary?.ETDDate,
                    ShipFrom = toUpdateShipment.ShipFrom,
                    ShipFromETDDate = toUpdateShipment.ShipFromETDDate,
                    ShipTo = toUpdateShipment.ShipTo,
                    ShipToETADate = toUpdateShipment.ShipToETADate.Value,
                    Movement = toUpdateShipment.Movement,
                    CarrierSONo = containerVM.CarrierBookingNo,
                    TotalGrossWeightUOM = AppConstant.KILOGGRAMS,
                    TotalNetWeightUOM = AppConstant.KILOGGRAMS,
                    TotalVolumeUOM = AppConstant.CUBIC_METER,
                    SealNo = containerVM.SealNo,
                    SealNo2 = containerVM.SealNo2,
                    IsFCL = toUpdateShipment.IsFCL,
                    CreatedBy = toUpdateShipment.CreatedBy,
                    CreatedDate = toUpdateShipment.CreatedDate,
                    UpdatedBy = toUpdateShipment.UpdatedBy,
                    UpdatedDate = toUpdateShipment.UpdatedDate,
                    ContainerItineraries = new List<ContainerItineraryModel>(),
                    ShipmentLoads = new List<ShipmentLoadModel>()
                };
                var newShipmentLoad = new ShipmentLoadModel
                {
                    EquipmentType = newContainer.ContainerType,
                    ModeOfTransport = toUpdateShipment.ModeOfTransport,
                    CarrierBookingNo = newContainer.CarrierSONo,
                    IsFCL = newContainer.IsFCL,
                    CreatedBy = toUpdateShipment.CreatedBy,
                    CreatedDate = toUpdateShipment.CreatedDate,
                    UpdatedBy = toUpdateShipment.UpdatedBy,
                    UpdatedDate = toUpdateShipment.UpdatedDate,
                    Shipment = toUpdateShipment, // link to Shipments
                    ShipmentId = toUpdateShipment.Id,
                    Consignment = availableConsignment, // link to Consignments
                    Container = newContainer, // link to Containers
                    ShipmentLoadDetails = new List<ShipmentLoadDetailModel>(),
                    BillOfLadingShipmentLoads = new List<BillOfLadingShipmentLoadModel>()
                };
                // Link ShipmentLoad to Container
                newContainer.ShipmentLoads.Add(newShipmentLoad);

                // TotalPackageUOM is the 1st item of ShipmentLoadDetails
                var firstLoadDetail = importVM.CargoDetails?
                    .Where(cgd => cgd.LoadDetails != null && cgd.LoadDetails.Any())?
                    .SelectMany(cgd => cgd.LoadDetails)?
                    .FirstOrDefault(ld => ld.ContainerNo == newContainer.ContainerNo);

                if (firstLoadDetail != null)
                {
                    newContainer.TotalPackageUOM = firstLoadDetail.PackageUOM;
                }

                if (toUpdateShipment.IsFCL)
                {
                    newContainer.TotalGrossWeightUOM = toUpdateShipment.TotalGrossWeightUOM;
                    newContainer.TotalNetWeightUOM = toUpdateShipment.TotalNetWeightUOM;
                    newContainer.TotalVolumeUOM = toUpdateShipment.TotalVolumeUOM;

                    newContainer.LoadPlanRefNo = await GenerateLoadNumber(newContainer.CreatedDate);
                    toCreateContainers.Add(newContainer);
                }
                else
                {
                    var newConsolidation = new ConsolidationModel
                    {
                        OriginCFS = firstItinerary?.LoadingPort,
                        CFSCutoffDate = containerVM.CFSCutoffDate.Value, // required if IsFCL = false
                        ModeOfTransport = toUpdateShipment.ModeOfTransport,
                        TotalGrossWeightUOM = AppConstant.KILOGGRAMS,
                        TotalNetWeightUOM = AppConstant.KILOGGRAMS,
                        TotalVolumeUOM = AppConstant.CUBIC_METER,
                        Stage = ConsolidationStage.Confirmed,
                        CreatedBy = toUpdateShipment.CreatedBy,
                        CreatedDate = toUpdateShipment.CreatedDate,
                        UpdatedBy = toUpdateShipment.UpdatedBy,
                        UpdatedDate = toUpdateShipment.UpdatedDate,
                        ShipmentLoads = new List<ShipmentLoadModel>()
                    };
                    newShipmentLoad.Consolidation = newConsolidation;

                    var fromDate = DateTime.Today.AddDays(-10);
                    var toDate = DateTime.Today.AddDays(10);

                    var existingContainer = await _containerRepository.Query(c => c.ContainerNo == containerVM.ContainerNo && c.LoadingDate >= fromDate && c.LoadingDate <= toDate,
                                                                                includes: x => x.Include(y => y.ShipmentLoads)
                                                                                                .Include(y => y.Consolidation)
                                                                                                .Include(y => y.ContainerItineraries)
                                                                            ).FirstOrDefaultAsync();

                    // Not found, use new container object
                    if (existingContainer == null || toDeleteContainerIds.Contains(existingContainer.Id))
                    {
                        newConsolidation.PopulateFromContainer(newContainer);
                        newConsolidation.Container = newContainer;
                        newConsolidation.ConsolidationNo = await GenerateLoadPlanID(DateTime.UtcNow);
                        newConsolidation.ShipmentLoads = new[] { newShipmentLoad };

                        newContainer.Consolidation = newConsolidation;
                        newContainer.LoadPlanRefNo = await GenerateLoadNumber(newContainer.CreatedDate);

                        toCreateContainers.Add(newContainer);
                    }
                    else
                    {
                        // Found, use the exisiting container
                        newShipmentLoad.EquipmentType = existingContainer.ContainerType;
                        newShipmentLoad.CarrierBookingNo = existingContainer.CarrierSONo;

                        // If data of each API call is different, system will store the last data imported.                        
                        existingContainer.ContainerType = newContainer.ContainerType;
                        if (newContainer.LoadingDate.HasValue)
                        {
                            existingContainer.LoadingDate = newContainer.LoadingDate;
                        }
                        if (containerVM.IsPropertyDirty(nameof(ImportShipmentContainerViewModel.SealNo)))
                        {
                            existingContainer.SealNo = containerVM.SealNo;
                        }
                        if (containerVM.IsPropertyDirty(nameof(ImportShipmentContainerViewModel.SealNo2)))
                        {
                            existingContainer.SealNo2 = containerVM.SealNo2;
                        }
                        if (containerVM.IsPropertyDirty(nameof(ImportShipmentContainerViewModel.CarrierBookingNo)))
                        {
                            existingContainer.CarrierSONo = containerVM.CarrierBookingNo;
                        }

                        // Consolidations
                        if (existingContainer.Consolidation == null)
                        {
                            newConsolidation.PopulateFromContainer(existingContainer);

                            // Link between Container & Consolidation
                            newConsolidation.Container = existingContainer;
                            existingContainer.Consolidation = newConsolidation;

                            newConsolidation.ConsolidationNo = await GenerateLoadPlanID(DateTime.UtcNow);

                            // Add new consolidation
                            toCreateConsolidations.Add(newConsolidation);
                        }
                        else
                        {
                            newShipmentLoad.Consolidation = existingContainer.Consolidation;
                        }

                        toUpdateCFSContainers.Add(existingContainer);

                        // Link to ShipmentLoad
                        newShipmentLoad.ContainerId = existingContainer.Id;
                        newShipmentLoad.Container = existingContainer;

                        // Add new ShipmentLoad
                        toCreateShipmentLoads.Add(newShipmentLoad);

                        // Link to existing container
                        existingContainer.ShipmentLoads.Add(newShipmentLoad);
                        availableContainers.Add(existingContainer);
                    }
                }

                // Set for further uses
                availableShipmentLoads.Add(newShipmentLoad);
            }

            // Set for further uses
            availableContainers.AddRange(toCreateContainers);
        }
        else
        {
            // No containers in payload

        }
        #endregion Containers/ShipmentLoads/Consolidations

        #region ContainerItineraries

        // Loop to create ContainerItineraries
        if (availableContainers.Any() && availableItineraries.Any())
        {
            availableContainers.ForEach(container =>
            {
                availableItineraries.ForEach(itinerary =>
                {
                    var isExisting = itinerary.Id != 0 && container.ContainerItineraries.Any(x => x.ItineraryId == itinerary.Id);
                    if (!isExisting)
                    {
                        container.ContainerItineraries.Add(new ContainerItineraryModel
                        {
                            Container = container,
                            Itinerary = itinerary,
                            CreatedBy = toUpdateShipment.CreatedBy,
                            CreatedDate = toUpdateShipment.CreatedDate,
                            UpdatedBy = toUpdateShipment.UpdatedBy,
                            UpdatedDate = toUpdateShipment.UpdatedDate,
                        });
                    }
                });
            });
        }
        #endregion ContainerItineraries        

        #region MasterBillOfLadings

        var availableBillOfLadings = await _billOfLadingRepository.Query(x => x.ShipmentBillOfLadings.Any(x => x.ShipmentId == toUpdateShipment.Id),
                                                                       includes: x => x.Include(y => y.BillOfLadingConsignments)
                                                                                       .Include(y => y.ShipmentBillOfLadings)
                                                                                       .Include(y => y.BillOfLadingShipmentLoads)
                                                                                       .Include(y => y.BillOfLadingItineraries)
                                                                                       .Include(y => y.Contacts)
                                                                       ).ToListAsync() ?? default;

        var toCreateMasterBillOfLadings = new List<MasterBillOfLadingModel>();
        var toUpdateMasterBillOfLadings = new List<MasterBillOfLadingModel>();
        var toCreateMasterBillOfLadingItineraries = new List<MasterBillOfLadingItineraryModel>();

        var toUpdateBillOfLadingItineraries = new List<BillOfLadingItineraryModel>();
        var toUpdateConsignmentItineraries = new List<ConsignmentItineraryModel>();

        // Master bill of lading number in payload
        if (!string.IsNullOrEmpty(importVM.MasterNo))
        {
            // Consignments: remove MasterBillId
            availableConsignment.MasterBillId = null;
            availableConsignment.MasterBill = null;

            // BillOfLadingShipmentLoads: remove MasterBillId
            var billOfLadingShipmentLoads = availableBillOfLadings.SelectMany(x => x.BillOfLadingShipmentLoads).Where(x => availableShipmentLoads.Any(y => y.Id == x.ShipmentLoadId)).ToList();
            if (billOfLadingShipmentLoads != null && billOfLadingShipmentLoads.Any())
            {
                billOfLadingShipmentLoads.ForEach(x =>
                {
                    x.MasterBillOfLadingId = null;
                    x.MasterBillOfLading = null;
                });
            }
            toUpdateBillOfLadingShipmentLoads.AddRange(billOfLadingShipmentLoads);

            // BillOfLadingItineraries: remove MasterBillId
            var billOfLadingItineraries = availableBillOfLadings.SelectMany(x => x.BillOfLadingItineraries).Where(x => toUpdateItineraries.Any(y => y.Id == x.ItineraryId)).ToList();
            if (billOfLadingItineraries != null && billOfLadingItineraries.Any())
            {
                billOfLadingItineraries.ForEach(x =>
                {
                    x.MasterBillOfLadingId = null;
                    x.MasterBillOfLading = null;
                });
            }
            toUpdateBillOfLadingItineraries.AddRange(billOfLadingItineraries);

            // Get if existing new Master bill of lading number
            var masterBOL = await _masterBillOfLadingRepository.GetAsync(x => x.MasterBillOfLadingNo == importVM.MasterNo,
                                                                    includes: i => i.Include(y => y.Contacts)
                                                                                    .Include(y => y.MasterBillOfLadingItineraries));

            if (masterBOL == null)
            {
                masterBOL = new MasterBillOfLadingModel
                {
                    MasterBillOfLadingNo = importVM.MasterNo,
                    CreatedBy = userName,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedBy = userName,
                    UpdatedDate = DateTime.UtcNow,
                    Contacts = new List<MasterBillOfLadingContactModel>(),
                    MasterBillOfLadingItineraries = new List<MasterBillOfLadingItineraryModel>()
                };
            }
            else
            {
                masterBOL.Contacts.Clear();
                masterBOL.Audit(userName);
            }

            // If data of each API call is different, system will store the last data imported.
            masterBOL.IsDirectMaster = importVM.IsDirectMaster ?? false;

            if (!string.IsNullOrWhiteSpace(importVM.PlaceOfIssue))
            {
                masterBOL.PlaceOfIssue = importVM.PlaceOfIssue;
            }
            if (importVM.MasterIssueDate.HasValue)
            {
                masterBOL.IssueDate = importVM.MasterIssueDate.Value;
            }
            if (importVM.OnBoardDate.HasValue)
            {
                masterBOL.OnBoardDate = importVM.OnBoardDate.Value;
            }

            masterBOL.ModeOfTransport = toUpdateShipment.ModeOfTransport;
            masterBOL.Movement = toUpdateShipment.Movement;
            masterBOL.CarrierContractNo = toUpdateShipment.CarrierContractNo;

            // MasterBillOfLadingItineraries

            if (availableItineraries.Any())
            {
                foreach (var itinerary in availableItineraries)
                {
                    var isExisting = itinerary.Id != 0 && masterBOL.MasterBillOfLadingItineraries.Any(x => x.ItineraryId == itinerary.Id);
                    if (!isExisting)
                    {
                        var newMasterBillOfLadingItinerary = new MasterBillOfLadingItineraryModel
                        {
                            Itinerary = itinerary,
                            CreatedBy = userName,
                            CreatedDate = DateTime.UtcNow,
                            UpdatedBy = userName,
                            UpdatedDate = DateTime.UtcNow
                        };
                        masterBOL.MasterBillOfLadingItineraries.Add(newMasterBillOfLadingItinerary);
                        if (masterBOL.Id > 0)
                        {
                            toCreateMasterBillOfLadingItineraries.Add(newMasterBillOfLadingItinerary);
                        }
                    }
                }
            }

            // To link master bill to ConsignmentItineraries
            var availableConsignmentItineraries = availableItineraries.SelectMany(x => x.ConsignmentItineraries).Where(x => x.ShipmentId == toUpdateShipment.Id);
            if (availableConsignmentItineraries != null && availableConsignmentItineraries.Any())
            {
                foreach (var item in availableConsignmentItineraries)
                {
                    item.MasterBillId = masterBOL.Id;
                    item.MasterBill = masterBOL;
                }
            }

            if (masterBOL.Id == 0)
            {

                if (billOfLadingItineraries != null && billOfLadingItineraries.Any())
                {
                    billOfLadingItineraries.ForEach(x =>
                    {
                        x.MasterBillOfLading = masterBOL;
                        x.MasterBillOfLadingId = masterBOL.Id;
                    });
                }

                toCreateMasterBillOfLadings.Add(masterBOL);
                availableMasterBillOfLadings = toCreateMasterBillOfLadings;

            }
            else
            {
                if (billOfLadingItineraries != null && billOfLadingItineraries.Any())
                {
                    billOfLadingItineraries.ForEach(x =>
                    {
                        x.MasterBillOfLadingId = masterBOL.Id;
                        x.MasterBillOfLading = masterBOL;
                    });
                }

                toUpdateMasterBillOfLadings.Add(masterBOL);
                availableMasterBillOfLadings = toUpdateMasterBillOfLadings;

            }
        }
        
        // Update MasterBillContacts
        if ((importVM.Contacts != null && importVM.Contacts.Any()) || !string.IsNullOrEmpty(importVM.MasterNo))
        {
            foreach (var masterBill in availableMasterBillOfLadings)
            {
                // Set to empty
                masterBill.Contacts.Clear();

                foreach (var contact in toUpdateShipment.Contacts)
                {
                    if (contact.OrganizationRole == OrganizationRole.Principal
                        || contact.OrganizationRole == OrganizationRole.OriginAgent
                        || contact.OrganizationRole == OrganizationRole.DestinationAgent)
                    {
                        var newMasterBOLContact = new MasterBillOfLadingContactModel
                        {
                            CreatedBy = userName,
                            CreatedDate = DateTime.UtcNow,
                            UpdatedBy = userName,
                            UpdatedDate = DateTime.UtcNow,
                        };
                        newMasterBOLContact.PopulateFromShipmentContact(contact);
                        masterBill.Contacts.Add(newMasterBOLContact);
                    }
                }
            }
        }
        
        // Set MasterBillOfLading to Consignment
        // There is only one Masterbill available
        availableConsignment.MasterBill = availableMasterBillOfLadings.FirstOrDefault();
        availableConsignment.MasterBillId = availableMasterBillOfLadings.FirstOrDefault()?.Id;


        // ConsignmentItineraries: set MasterBillId
        var consignmentItineraries = toCreateItineraries.SelectMany(x => x.ConsignmentItineraries).Where(x => x.ConsignmentId == availableConsignment.Id).ToList();
        if (consignmentItineraries != null && consignmentItineraries.Any())
        {
            consignmentItineraries.ForEach(x =>
            {
                x.MasterBillId = availableMasterBillOfLadings.FirstOrDefault()?.Id;
                x.MasterBill = availableMasterBillOfLadings.FirstOrDefault();
            });
        }
        consignmentItineraries = toCreateConsignmentItineraries.ToList();
        if (consignmentItineraries != null && consignmentItineraries.Any())
        {
            consignmentItineraries.ForEach(x =>
            {
                x.MasterBillId = availableMasterBillOfLadings.FirstOrDefault()?.Id;
                x.MasterBill = availableMasterBillOfLadings.FirstOrDefault();
            });
        }

        //There is only on MasterBillOfLading
        var availableMasterBillOfLading = availableMasterBillOfLadings.FirstOrDefault();

        //Update MasterBill from Itinerary
        if (importVM.Itineraries != null && importVM.Itineraries.Any() || !string.IsNullOrEmpty(importVM.MasterNo))
        {
            if (firstItinerary != null && availableMasterBillOfLading != null)
            {
                availableMasterBillOfLading.SCAC = firstItinerary.SCAC;
                availableMasterBillOfLading.AirlineCode = firstItinerary.AirlineCode;
                availableMasterBillOfLading.CarrierName = firstItinerary.CarrierName;
                availableMasterBillOfLading.VesselFlight = firstItinerary.VesselFlight;
                availableMasterBillOfLading.Vessel = firstItinerary.VesselName;
                availableMasterBillOfLading.Voyage = firstItinerary.Voyage;
                availableMasterBillOfLading.FlightNo = firstItinerary.FlightNumber;
                availableMasterBillOfLading.PlaceOfReceipt = firstItinerary.LoadingPort;
                availableMasterBillOfLading.PortOfLoading = firstItinerary.LoadingPort;
                availableMasterBillOfLading.PlaceOfDelivery = firstItinerary.DischargePort;
                availableMasterBillOfLading.PortOfDischarge = firstItinerary.DischargePort;
                availableMasterBillOfLading.PlaceOfIssue = firstItinerary.LoadingPort;
                availableMasterBillOfLading.IssueDate = firstItinerary.ETDDate;
                availableMasterBillOfLading.OnBoardDate = firstItinerary.ETDDate;
            }
        }
        #endregion MasterBillOfLadings

        #region BillOfLadings

        var toCreateBillOfLadingContacts = new List<BillOfLadingContactModel>();
        var toCreateBillOfLadings = new List<BillOfLadingModel>();
        var toUpdateBillOfLadings = new List<BillOfLadingModel>();
        var toDeleteBillOfLadingConsignments = new List<BillOfLadingConsignmentModel>();
        var toCreateBillOfLadingConsignments = new List<BillOfLadingConsignmentModel>();
        var toDeleteShipmentBillOfLadings = new List<ShipmentBillOfLadingModel>();
        var toCreateShipmentBillOfLadings = new List<ShipmentBillOfLadingModel>();
        var toCreateBillOfLadingItineraries = new List<BillOfLadingItineraryModel>();
        var toCreateBillOfLadingShipmentLoads = new List<BillOfLadingShipmentLoadModel>();

        //var availableBillOfLadingNos = availableBillOfLadings.Select(x => x.BillOfLadingNo).ToList();
        // && !availableBillOfLadingNos.Contains(importVM.HouseNo)

        // BillOfLading number in payload
        if (!string.IsNullOrEmpty(importVM.HouseNo))
        {
            // BillOfLadingConsignments: remove record
            toDeleteBillOfLadingConsignments = availableBillOfLadings
                .Where(x => x.BillOfLadingNo != importVM.HouseNo)
                .SelectMany(x => x.BillOfLadingConsignments).Where(x => x.ConsignmentId == availableConsignment.Id).ToList();

            // ShipmentBillOfLadings: remove record
            toDeleteShipmentBillOfLadings = availableBillOfLadings
                .Where(x => x.BillOfLadingNo != importVM.HouseNo)
                .SelectMany(x => x.ShipmentBillOfLadings).Where(x => x.ShipmentId == toUpdateShipment.Id).ToList();

            // BillOfLadingShipmentLoads: remove record
            toUpdateBillOfLadingShipmentLoads = new List<BillOfLadingShipmentLoadModel>();
            toDeleteBillOfLadingShipmentLoads.AddRange(availableBillOfLadings.SelectMany(x => x.BillOfLadingShipmentLoads).Where(x => toDeleteShipmentLoads.Any(y => y.Id == x.ShipmentLoadId)
                                                                                                                                || availableShipmentLoads.Any(y => y.Id == x.ShipmentLoadId)).ToList());

            // Consignments: remove HouseBLId
            availableConsignment.HouseBillId = null;
            availableConsignment.HouseBill = null;

            // Re-calculate Weight, Volume, Unit, Package of the unlink Container, Consolidation, House BL correspondingly.
            foreach (var item in availableBillOfLadings)
            {
                item.TotalGrossWeight -= toUpdateShipment.TotalGrossWeight;
                item.TotalNetWeight -= toUpdateShipment.TotalNetWeight;
                item.TotalVolume -= toUpdateShipment.TotalVolume;
                item.TotalPackage -= toUpdateShipment.TotalPackage;
            }

            toUpdateBillOfLadings.AddRange(availableBillOfLadings);

            // Set to empty
            availableBillOfLadings.Clear();

            // Get if existing new Bill of lading number
            var billOfLading = await _billOfLadingRepository.GetAsync(x => x.BillOfLadingNo == importVM.HouseNo,
                includes: i => i.Include(y => y.Contacts).Include(y => y.ShipmentBillOfLadings).Include(y => y.BillOfLadingItineraries));

            if (billOfLading == null)
            {
                billOfLading = new BillOfLadingModel
                {
                    BillOfLadingNo = importVM.HouseNo,
                    BillOfLadingType = importVM.HouseBillType,
                    JobNumber = importVM.JobNumber,
                    Incoterm = toUpdateShipment.Incoterm,
                    IssueDate = importVM.HouseIssueDate ?? default,
                    CreatedBy = userName,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedBy = userName,
                    UpdatedDate = DateTime.UtcNow,
                    Contacts = new List<BillOfLadingContactModel>(),
                    BillOfLadingConsignments = new List<BillOfLadingConsignmentModel>(),
                    ShipmentBillOfLadings = new List<ShipmentBillOfLadingModel>(),
                    BillOfLadingItineraries = new List<BillOfLadingItineraryModel>(),
                };
            }
            else
            {
                if (importVM.IsPropertyDirty(nameof(importVM.HouseBillType)))
                {
                    billOfLading.BillOfLadingType = importVM.HouseBillType;
                }
                if (importVM.IsPropertyDirty(nameof(importVM.JobNumber)))
                {
                    billOfLading.JobNumber = importVM.JobNumber;
                }
                if (importVM.IsPropertyDirty(nameof(importVM.Incoterm)))
                {
                    billOfLading.Incoterm = importVM.Incoterm;
                }
                billOfLading.Contacts.Clear();
                billOfLading.Audit(userName);
            }

            // Recalculate total bill
            billOfLading.TotalGrossWeight += toUpdateShipment.TotalGrossWeight;
            billOfLading.TotalGrossWeightUOM = AppConstant.KILOGGRAMS;
            billOfLading.TotalNetWeight += toUpdateShipment.TotalNetWeight;
            billOfLading.TotalNetWeightUOM = AppConstant.KILOGGRAMS;
            billOfLading.TotalPackage += toUpdateShipment.TotalPackage;
            billOfLading.TotalPackageUOM = toUpdateShipment.TotalPackageUOM;
            billOfLading.TotalVolume += toUpdateShipment.TotalVolume;
            billOfLading.TotalVolumeUOM = AppConstant.CUBIC_METER;

            // If data of each API call is different, system will store the last data imported.
            if (firstItinerary != null)
            {
                billOfLading.MainCarrier = firstItinerary.CarrierName;
                billOfLading.MainVessel = firstItinerary.VesselName;
                billOfLading.IssueDate = firstItinerary.ETDDate;
            }
            if (importVM.IsPropertyDirty(nameof(importVM.HouseIssueDate))
                && importVM.HouseIssueDate.HasValue)
            {
                billOfLading.IssueDate = importVM.HouseIssueDate.Value;
            }
            billOfLading.ModeOfTransport = toUpdateShipment.ModeOfTransport;
            billOfLading.ShipFrom = toUpdateShipment.ShipFrom;
            billOfLading.ShipTo = toUpdateShipment.ShipTo;
            billOfLading.ShipFromETDDate = toUpdateShipment.ShipFromETDDate;
            billOfLading.ShipToETADate = toUpdateShipment.ShipToETADate.Value;
            billOfLading.Movement = toUpdateShipment.Movement;

            var isExisting = false;

            // [ShipmentBillOfLadings]
            var newShipmentBillOfLading = new ShipmentBillOfLadingModel
            {
                ShipmentId = toUpdateShipment.Id,
                CreatedBy = userName,
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = userName,
                UpdatedDate = DateTime.UtcNow,
                BillOfLading = billOfLading,
                BillOfLadingId = billOfLading.Id
            };

            isExisting = billOfLading.Id > 0 && billOfLading.ShipmentBillOfLadings.SingleOrDefault(x => x.ShipmentId == newShipmentBillOfLading.ShipmentId) != null;
            if (!isExisting)
            {
                billOfLading.ShipmentBillOfLadings.Add(newShipmentBillOfLading);

                if (billOfLading.Id > 0)
                {
                    toCreateShipmentBillOfLadings.Add(newShipmentBillOfLading);
                }
            }
            else
            {
                // Has linked 
            }

            // [BillOfLadingConsignments]
            var newBillOfLadingConsignment = new BillOfLadingConsignmentModel
            {
                Consignment = availableConsignment,
                Shipment = toUpdateShipment,
                CreatedBy = userName,
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = userName,
                UpdatedDate = DateTime.UtcNow,
                BillOfLading = billOfLading,
                BillOfLadingId = billOfLading.Id
            };
            if (billOfLading.BillOfLadingConsignments == null)
            {
                billOfLading.BillOfLadingConsignments = new List<BillOfLadingConsignmentModel>();
            }

            isExisting = billOfLading.Id > 0 && billOfLading.BillOfLadingConsignments.SingleOrDefault(x => x.ShipmentId == newShipmentBillOfLading.ShipmentId) != null;
            if (!isExisting)
            {
                billOfLading.BillOfLadingConsignments.Add(newBillOfLadingConsignment);

                if (billOfLading.Id > 0)
                {
                    toCreateBillOfLadingConsignments.Add(newBillOfLadingConsignment);
                }
            }
            else
            {
                // Has linked
            }

            // BillOfLadingShipmentLoad
            if (availableShipmentLoads.Any())
            {
                foreach (var shipmentLoad in availableShipmentLoads)
                {
                    var newBillOfLadingShipmentLoad = new BillOfLadingShipmentLoadModel
                    {
                        BillOfLading = billOfLading,
                        Container = shipmentLoad.Container,
                        Consolidation = shipmentLoad.Consolidation,
                        MasterBillOfLading = availableMasterBillOfLading,
                        MasterBillOfLadingId = availableMasterBillOfLading?.Id,
                        ShipmentLoad = shipmentLoad,
                        ShipmentLoadId = shipmentLoad.Id,
                        CreatedBy = userName,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedBy = userName,
                        UpdatedDate = DateTime.UtcNow,
                    };
                    var isExistingBillOfLadingShipmentLoad = shipmentLoad.Id > 0 && billOfLading.Id > 0
                        && shipmentLoad.BillOfLadingShipmentLoads.SingleOrDefault(x => x.BillOfLadingId == billOfLading.Id) != null;

                    if (!isExistingBillOfLadingShipmentLoad)
                    {
                        shipmentLoad.BillOfLadingShipmentLoads?.Add(newBillOfLadingShipmentLoad);

                        if (shipmentLoad.Id > 0)
                        {
                            toCreateBillOfLadingShipmentLoads.Add(newBillOfLadingShipmentLoad);
                        }
                    }
                }
            }

            // Switch to create or update BillOfLading
            if (billOfLading.Id == 0)
            {
                toCreateBillOfLadings.Add(billOfLading);
            }
            else
            {
                // Not already
                if (!toUpdateBillOfLadings.Any(x => x.Id == billOfLading.Id))
                {
                    toUpdateBillOfLadings.Add(billOfLading);
                }
            }
            // add for further uses
            availableBillOfLadings.Add(billOfLading);
        }
        else
        {
            //...there is no HouseNo in payload...

            // to update current BL
            if (availableBillOfLadings != null && availableBillOfLadings.Any())
            {
                foreach (var item in availableBillOfLadings)
                {
                    if (importVM.IsPropertyDirty(nameof(importVM.HouseBillType)))
                    {
                        item.BillOfLadingType = importVM.HouseBillType;
                    }
                    if (importVM.IsPropertyDirty(nameof(importVM.JobNumber)))
                    {
                        item.JobNumber = importVM.JobNumber;
                    }
                    if (importVM.IsPropertyDirty(nameof(importVM.Incoterm)))
                    {
                        item.Incoterm = importVM.Incoterm;
                    }
                }
            }
        }

        // Set BillOfLading to Consignment
        // There is only one Housebill available
        availableConsignment.HouseBill = availableBillOfLadings.FirstOrDefault();
        availableConsignment.HouseBillId = availableBillOfLadings.FirstOrDefault()?.Id;

        // Update BillOfLadingContacts
        if ((importVM.Contacts != null && importVM.Contacts.Any()) || !string.IsNullOrEmpty(importVM.HouseNo))
        {
            foreach (var houseBill in availableBillOfLadings)
            {
                // Set to empty.
                houseBill.Contacts.Clear();

                foreach (var newContact in toUpdateShipment.Contacts)
                {
                    var newBillOfLadingContact = new BillOfLadingContactModel
                    {
                        CreatedBy = userName,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedBy = userName,
                        UpdatedDate = DateTime.UtcNow,
                        BillOfLading = houseBill,
                        BillOfLadingId = houseBill.Id
                    };
                    newBillOfLadingContact.PopulateFromShipmentContact(newContact);

                    houseBill.Contacts.Add(newBillOfLadingContact);

                    if (houseBill.Id > 0)
                    {
                        toCreateBillOfLadingContacts.Add(newBillOfLadingContact);
                    }
                }
            }
        }

        //BillOfLadingItineraries
        if (availableItineraries != null && availableItineraries.Any() &&
            availableBillOfLadings != null && availableBillOfLadings.Any())
        {
            foreach (var billOfLading in availableBillOfLadings)
            {
                foreach (var itinerary in availableItineraries)
                {
                    var availableBillOfLadingItineraries = billOfLading.BillOfLadingItineraries.Where(x => x.ItineraryId == itinerary.Id);
                    var isExisting = itinerary.Id != 0 && availableBillOfLadingItineraries != null && availableBillOfLadingItineraries.Any();
                    if (!isExisting)
                    {
                        var newBillOfLadingItinerary = new BillOfLadingItineraryModel
                        {
                            Itinerary = itinerary,
                            CreatedBy = userName,
                            CreatedDate = DateTime.UtcNow,
                            UpdatedBy = userName,
                            UpdatedDate = DateTime.UtcNow,
                            MasterBillOfLading = availableMasterBillOfLading,
                            MasterBillOfLadingId = availableMasterBillOfLading?.Id
                        };
                        billOfLading.BillOfLadingItineraries.Add(newBillOfLadingItinerary);
                        if (billOfLading.Id > 0)
                        {
                            toCreateBillOfLadingItineraries.Add(newBillOfLadingItinerary);
                        }
                    }
                    else
                    {
                        foreach (var item in availableBillOfLadingItineraries)
                        {
                            item.MasterBillOfLading = availableMasterBillOfLading;
                            item.MasterBillOfLadingId = availableMasterBillOfLading?.Id;
                        }
                        toUpdateBillOfLadingItineraries.AddRange(availableBillOfLadingItineraries);
                    }
                }
            }
        }

        // BillOfLadingShipmentLoad
        if (toCreateBillOfLadingShipmentLoads.Any())
        {
            foreach (var item in toCreateBillOfLadingShipmentLoads)
            {
                item.MasterBillOfLading = availableMasterBillOfLadings.FirstOrDefault();
            }
        }

        // BillOfLadingShipmentLoad
        if (toUpdateBillOfLadingShipmentLoads.Any())
        {
            foreach (var item in toUpdateBillOfLadingShipmentLoads)
            {
                item.MasterBillOfLading = availableMasterBillOfLadings.FirstOrDefault();
            }
        }

        #endregion BillOfLadings              

        #region CargoDetails & ShipmentLoadDetails

        var toDeleteCargoDetails = new List<CargoDetailModel>();
        var toUpdateShipmentLoadDetails = new List<ShipmentLoadDetailModel>();

        var availableCargoDetails = _cargoDetailRepository.Query(x => x.ShipmentId == toUpdateShipment.Id,
                                                                    includes: x => x.Include(y => y.ShipmentLoadDetails)
                                                                ).ToList();
        var availableShipmentLoadDetails = availableContainers.Where(x => x.ShipmentLoadDetails != null).SelectMany(x => x.ShipmentLoadDetails).Where(x => x.ShipmentId == toUpdateShipment.Id).ToList();
        if (importVM.CargoDetails != null && importVM.CargoDetails.Any())
        {
            // Delete current cargo details
            toDeleteCargoDetails = _cargoDetailRepository.Query(x => x.ShipmentId == toUpdateShipment.Id).ToList();

            // Removing CargoDetails then remove ShipmentLoadDetails also
            // Update Containers will remove ShipmentLoadDetails -> Add more values
            toDeleteShipmentLoadDetails.AddRange(availableCargoDetails.SelectMany(x => x.ShipmentLoadDetails).ToList());
            toDeleteShipmentLoadDetails.AddRange(availableShipmentLoadDetails);

            foreach (var item in availableShipmentLoads)
            {
                item.ShipmentLoadDetails = new List<ShipmentLoadDetailModel>();
            }

            // Then create new list
            foreach (var cargoDetailVM in importVM.CargoDetails)
            {
                var newCargoDetailModel = new CargoDetailModel();

                Mapper.Map(cargoDetailVM, newCargoDetailModel);
                newCargoDetailModel.Shipment = toUpdateShipment;
                newCargoDetailModel.ShipmentId = toUpdateShipment.Id;
                newCargoDetailModel.OrderType = toUpdateShipment.OrderType;
                newCargoDetailModel.ShipmentLoadDetails = new List<ShipmentLoadDetailModel>();

                // Load Details
                if (cargoDetailVM.LoadDetails != null && !isAirShipment) // as business rule, there is no "Load Details" for AIR
                {
                    foreach (var loadDetailVM in cargoDetailVM.LoadDetails)
                    {
                        ShipmentLoadDetailModel newShipmentLoadDetail = new();

                        Mapper.Map(loadDetailVM, newShipmentLoadDetail);
                        newShipmentLoadDetail.Audit(userName);

                        var shipmentLoad = availableShipmentLoads.SingleOrDefault(x => x.Container.ContainerNo == loadDetailVM.ContainerNo);
                        if (shipmentLoad != null)
                        {
                            var container = shipmentLoad.Container;

                            container.TotalGrossWeight += loadDetailVM.GrossWeight ?? 0;
                            container.TotalNetWeight += loadDetailVM.NetWeight ?? 0;
                            container.TotalPackage += (int?)loadDetailVM.Package ?? 0;
                            container.TotalVolume += loadDetailVM.Volume ?? 0;

                            if (!container.IsFCL && container.Consolidation != null)
                            {
                                container.Consolidation.TotalGrossWeight += loadDetailVM.GrossWeight ?? 0;
                                container.Consolidation.TotalNetWeight += loadDetailVM.NetWeight ?? 0;
                                container.Consolidation.TotalPackage += (int?)loadDetailVM.Package ?? 0;
                                container.Consolidation.TotalVolume += loadDetailVM.Volume ?? 0;

                                newShipmentLoadDetail.Consolidation = container.Consolidation; // link to consolidation
                            }

                            // Add new ShipmentLoadDetails to current available ShipmentLoads
                            shipmentLoad.ShipmentLoadDetails.Add(newShipmentLoadDetail);

                            newShipmentLoadDetail.ShipmentLoadId = shipmentLoad.Id;
                            newShipmentLoadDetail.ShipmentLoad = shipmentLoad;
                            newShipmentLoadDetail.ConsignmentId = availableConsignment.Id;
                            newShipmentLoadDetail.Consignment = availableConsignment;
                            newShipmentLoadDetail.ShipmentId = toUpdateShipment.Id;
                            newShipmentLoadDetail.Shipment = toUpdateShipment;
                            newShipmentLoadDetail.Container = container;
                            newShipmentLoadDetail.ContainerId = container.Id;

                            // As creating new CargoDetail -> ShipmentLoadDetail will created also
                            newCargoDetailModel.ShipmentLoadDetails.Add(newShipmentLoadDetail);
                        }
                    }
                }

                newCargoDetailModel.Audit(userName);
                toCreateCargoDetails.Add(newCargoDetailModel);
            }
            availableCargoDetails = toCreateCargoDetails;
        }

        // Set ConsolodationId to ShipmentLoadDetails
        // Check not new ShipmentLoadDetails
        var availableShipmentLoadsDetails = availableShipmentLoads.Where(x => x.ShipmentLoadDetails != null).SelectMany(x => x.ShipmentLoadDetails).Where(x => x.Id != 0);

        if (availableShipmentLoadsDetails != null && availableShipmentLoadsDetails.Any())
        {
            foreach (var item in availableShipmentLoadsDetails)
            {
                item.ConsolidationId = availableContainers.Find(x => x.Id == item.ContainerId)?.Consolidation?.Id;
                item.Consolidation = availableContainers.Find(x => x.Id == item.ContainerId)?.Consolidation;
            }
            toUpdateShipmentLoadDetails.AddRange(availableShipmentLoadsDetails);
        }

        #endregion CargoDetails & ShipmentLoadDetails

        #region Update some information silent

        // Update some more info silent
        try
        {
            // Update BillOfLadings.ExecutionAgentId
            if (availableBillOfLadings != null && availableBillOfLadings.Any())
            {
                foreach (var item in availableBillOfLadings)
                {
                    item.ExecutionAgentId = item?.Contacts.FirstOrDefault(x => x.OrganizationRole == OrganizationRole.OriginAgent)?.OrganizationId ?? 0;
                }
            }

            // Update MasterBills.ExecutionAgentId
            if (availableMasterBillOfLadings != null && availableMasterBillOfLadings.Any())
            {
                foreach (var item in availableMasterBillOfLadings)
                {
                    item.ExecutionAgentId = item?.Contacts.FirstOrDefault(x => x.OrganizationRole == OrganizationRole.OriginAgent)?.OrganizationId ?? 0;
                }
            }

            // Update Consignment.ExecutionAgentId
            if (toUpdateShipment.Consignments != null && toUpdateShipment.Consignments.Any())
            {
                if (toUpdateShipment.Contacts != null && toUpdateShipment.Contacts.Any())
                {
                    var originAgent = toUpdateShipment.Contacts.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.OriginAgent);
                    if (originAgent != null)
                    {
                        string executionAgentName = null;
                        if (originAgent.OrganizationId != 0)
                        {
                            var originAgentOrg = await _csfeApiClient.GetOrganizationByIdAsync(originAgent.OrganizationId);
                            executionAgentName = originAgentOrg?.Name;
                        }
                        foreach (var item in toUpdateShipment.Consignments)
                        {
                            item.ExecutionAgentId = originAgent.OrganizationId;
                            item.ExecutionAgentName = executionAgentName;
                        }
                    }
                }
            }
        }
        catch
        {

        }

        #endregion Update some information silent

        #region Update to database

        UnitOfWork.BeginTransaction();

        // ShipmentContacts
        _shipmentContactRepository.RemoveRange(toDeleteShipmentContacts.ToArray());
        await _shipmentContactRepository.AddRangeAsync(toCreateShipmentContacts.ToArray());

        // Consignments
        await _consignmentRepository.AddRangeAsync(toCreateConsignments.ToArray());
        _consignmentRepository.UpdateRange(toUpdateConsignments.ToArray());

        // Itineraries
        _billOfLadingItineraryRepository.RemoveRange(toDeteleBillOfLadingItineraries.ToArray());
        toDeleteItineraries.ForEach(x =>
        {
            // Clear all relationships before deleting
            x.ConsignmentItineraries?.Clear();
            x.ContainerItineraries?.Clear();
            x.MasterBillOfLadingItineraries?.Clear();
            x.BillOfLadingItineraries?.Clear();
        });
        _itineraryRepository.RemoveRange(toDeleteItineraries.ToArray());
        _itineraryRepository.UpdateRange(toUpdateItineraries.ToArray());
        await _itineraryRepository.AddRangeAsync(toCreateItineraries.ToArray());

        // To unlink sharing itineraries
        _consignmentItineraryRepository.RemoveRange(toDeleteConsignmentItineraries.ToArray());
        // To link/share existing itineraries
        await _consignmentItineraryRepository.AddRangeAsync(toCreateConsignmentItineraries.ToArray());
        
        // Containers
        _billOfLadingShipmentLoadRepository.RemoveRange(toDeleteBillOfLadingShipmentLoads.ToArray());
        _billOfLadingShipmentLoadRepository.UpdateRange(toUpdateBillOfLadingShipmentLoads.ToArray());

        // Remove ShipmentLoadDetail
        _shipmentLoadDetailRepository.RemoveRange(toDeleteShipmentLoadDetails.ToArray());
        _shipmentLoadDetailRepository.UpdateRange(toUpdateShipmentLoadDetails.ToArray());

        // Removing ShipmentLoad
        toDeleteShipmentLoads.ForEach(x => x.BillOfLadingShipmentLoads?.Clear());
        _shipmentLoadRepository.RemoveRange(toDeleteShipmentLoads.ToArray());

        await _shipmentLoadRepository.AddRangeAsync(toCreateShipmentLoads.ToArray());

        // Removing ContainerItinerary
        _containerItineraryRepository.RemoveRange(toDeleteContainerItineraries.ToArray());

        _consolidationRepository.RemoveRange(toDeleteConsolidations.ToArray());
        // toDeleteContainers = CY and also not-sharing CFS
        _containerRepository.RemoveRange(toDeleteContainers.ToArray());

        // To update Weight, Volume, Unit, Package of the unlink Container, Consolidation
        _consolidationRepository.UpdateRange(toUpdateConsolidations.ToArray());
        _containerRepository.UpdateRange(toUpdateCFSContainers.ToArray());

        await _consolidationRepository.AddRangeAsync(toCreateConsolidations.ToArray());
        await _containerRepository.AddRangeAsync(toCreateContainers.ToArray());

        // BillOfLading
        _shipmentBillOfLadingRepository.RemoveRange(toDeleteShipmentBillOfLadings.ToArray());
        _billOfLadingConsignmentRepository.RemoveRange(toDeleteBillOfLadingConsignments.ToArray());

        await _billOfLadingContactRepository.AddRangeAsync(toCreateBillOfLadingContacts.ToArray());

        await _shipmentBillOfLadingRepository.AddRangeAsync(toCreateShipmentBillOfLadings.ToArray());
        await _billOfLadingConsignmentRepository.AddRangeAsync(toCreateBillOfLadingConsignments.ToArray());
        await _billOfLadingItineraryRepository.AddRangeAsync(toCreateBillOfLadingItineraries.ToArray());
        await _billOfLadingShipmentLoadRepository.AddRangeAsync(toCreateBillOfLadingShipmentLoads.ToArray());


        // To update Weight, Volume, Unit, Package of the unlink House BL
        _billOfLadingRepository.UpdateRange(toUpdateBillOfLadings.ToArray());

        await _billOfLadingRepository.AddRangeAsync(toCreateBillOfLadings.ToArray());

        // MasterBillOfLading
        _consignmentItineraryRepository.UpdateRange(toUpdateConsignmentItineraries.ToArray());

        _billOfLadingItineraryRepository.UpdateRange(toUpdateBillOfLadingItineraries.ToArray());

        _masterBillOfLadingRepository.UpdateRange(toUpdateMasterBillOfLadings.Distinct().ToArray());

        await _masterBillOfLadingItineraryRepository.AddRangeAsync(toCreateMasterBillOfLadingItineraries.ToArray());

        await _masterBillOfLadingRepository.AddRangeAsync(toCreateMasterBillOfLadings.ToArray());

        //Shipment
        Repository.Update(toUpdateShipment);

        await UnitOfWork.SaveChangesAsync();

        /* 
         * Warning: to "trg_CargoDetails.sql" work correctly, Please make sure that Contacts is added before CargoDetails.
         */

        // CargoDetails & ShipmentLoadDetails
        _cargoDetailRepository.RemoveRange(toDeleteCargoDetails.ToArray());
        await _cargoDetailRepository.AddRangeAsync(toCreateCargoDetails.ToArray());
        await UnitOfWork.SaveChangesAsync();
        // POFulfillmentContacts
        _poFulfillmentContactRepository.RemoveRange(toDeletePOFulfillmentContacts.ToArray());
        await _poFulfillmentContactRepository.AddRangeAsync(toCreatePOFulfillmentContacts.ToArray());

        await UnitOfWork.SaveChangesAsync();

        #region Trigger/Update/Delete Departure/Arrival event

        var toCreateActivities = new List<ActivityViewModel>();
        var toUpdateActivities = new List<Tuple<long, ActivityViewModel>>();
        var toDeleteActivityIds = new List<long>();

        foreach (var freightSchedulerEvent in freightSchedulerEventList)
        {
            var eventCode = freightSchedulerEvent.Item1;
            var freightScheduler = freightSchedulerEvent.Item2;
            var departureEventCodes = new string[]
            {
                Event.EVENT_7001, // for SEA
                Event.EVENT_7003 // for AIR
            };
            var arrivalEventCodes = new string[]
            {
                Event.EVENT_7002, // for SEA
                Event.EVENT_7004 // for AIR
            };
            var location = departureEventCodes.Contains(eventCode) ? freightScheduler.LocationFromName : freightScheduler.LocationToName;

            DateTime activityDate = default;

            if (departureEventCodes.Contains(eventCode) && freightScheduler.ATDDate.HasValue)
            {
                activityDate = freightScheduler.ATDDate.Value;
            }
            else if (arrivalEventCodes.Contains(eventCode) && freightScheduler.ATADate.HasValue)
            {
                activityDate = freightScheduler.ATADate.Value;
            }

            var eventModel = await _csfeApiClient.GetEventByCodeAsync(eventCode);
            if (freightSchedulerEvent.Item3 == NEW_EVENT)
            {
                var newActivityVM = new ActivityViewModel
                {
                    FreightSchedulerId = freightScheduler.Id,
                    ActivityCode = eventCode,
                    ActivityType = eventModel.ActivityType,
                    ActivityDescription = eventModel.ActivityDescription,
                    Location = location,
                    ActivityDate = activityDate,
                    CreatedBy = userName
                };
                toCreateActivities.Add(newActivityVM);
            }
            if (freightSchedulerEvent.Item3 == UPDATE_EVENT || freightSchedulerEvent.Item3 == DELETE_EVENT)
            {

                var globalId = CommonHelper.GenerateGlobalId(freightScheduler.Id, EntityType.FreightScheduler);
                var activities = await _activityRepository.Query(s => s.GlobalIdActivities.Any(g => g.GlobalId == globalId)).ToListAsync();

                var activityInSystem = activities?.Find(c => c.ActivityCode == freightSchedulerEvent.Item1);

                if (freightSchedulerEvent.Item3 == UPDATE_EVENT)
                {
                    if (activityInSystem != null)
                    {
                        var activityVM = new ActivityViewModel
                        {
                            FreightSchedulerId = freightScheduler.Id,
                            ActivityCode = eventCode,
                            Location = location,
                            ActivityDate = activityDate,
                            UpdatedBy = userName
                        };

                        activityVM.Id = activityInSystem.Id;

                        // to work with Auto mapper
                        activityVM.FieldStatus = new Dictionary<string, FieldDeserializationStatus>
                                                            {
                                                                {
                                                                    nameof(ActivityViewModel.UpdatedBy), FieldDeserializationStatus.HasValue
                                                                },
                                                                {
                                                                    nameof(ActivityViewModel.UpdatedDate), FieldDeserializationStatus.HasValue
                                                                },
                                                                {
                                                                    nameof(ActivityViewModel.ActivityDate), FieldDeserializationStatus.HasValue
                                                                }
                                                            };
                        toUpdateActivities.Add(new Tuple<long, ActivityViewModel>(activityVM.Id, activityVM));
                    }
                    else
                    {
                        var newActivityVM = new ActivityViewModel
                        {
                            FreightSchedulerId = freightScheduler.Id,
                            ActivityCode = eventCode,
                            ActivityType = eventModel.ActivityType,
                            ActivityDescription = eventModel.ActivityDescription,
                            Location = location,
                            ActivityDate = activityDate,
                            CreatedBy = userName
                        };

                        toCreateActivities.Add(newActivityVM);
                    }
                }
                else
                {
                    if (activityInSystem != null)
                    {
                        toDeleteActivityIds.Add(activityInSystem.Id);
                    }
                }
            }
        }
        #endregion Trigger/Update/Delete Departure/Arrival event

        // Activities
        foreach (var item in toDeleteActivityIds)
        {
            await _activityService.DeleteByKeysAsync(item);
        }
        foreach (var item in toUpdateActivities)
        {
            await _activityService.UpdateAsync(item.Item2, item.Item1);
        }
        foreach (var item in toCreateActivities)
        {
            await _activityService.CreateAsync(item);
        }

        UnitOfWork.CommitTransaction();

        if (broadcastFreightScheduleIdList.Count > 0)
        {
            await _freightSchedulerService.BroadcastFreightScheduleUpdatesAsync(broadcastFreightScheduleIdList.Select(x => x.Id), Schedulers.UpdatedViaFreightSchedulerAPI, false, userName);
        }

        #endregion Update to database

        importingResult.LogSuccess("Id", $"{toUpdateShipment.Id}");
        importingResult.LogSuccess("Number", $"{toUpdateShipment.ShipmentNo}");
        importingResult.LogSuccess("Url", $"{_appConfig.ClientUrl}/shipments/{toUpdateShipment.Id}");

        return importingResult;
    }
}
