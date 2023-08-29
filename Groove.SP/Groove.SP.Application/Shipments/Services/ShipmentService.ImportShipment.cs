using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.Container.ViewModels;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Itinerary.ViewModels;
using Groove.SP.Application.ShipmentContact.Mappers;
using Groove.SP.Application.ShipmentContact.ViewModels;
using Groove.SP.Application.Shipments.ViewModels;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Groove.SP.Application.Shipments.Services;

public partial class ShipmentService
{
    public async Task<ImportingShipmentResultViewModel> ImportFreightShipmentAsync(ImportShipmentViewModel importVM, string userName)
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

        switch (importVM.Status)
        {
            case ImportShipmentStatus.N:

                #region Shipments
                var newShipment = new ShipmentModel
                {
                    ShipmentNo = importVM.ShipmentNo,
                    BuyerCode = importVM.BuyerCode,
                    CustomerReferenceNo = importVM.CustomerReferenceNo,
                    AgentReferenceNo = importVM.AgentReferenceNo,
                    ShipperReferenceNo = importVM.ShipperReferenceNo,
                    CarrierContractNo = importVM.CarrierContractNo,
                    ModeOfTransport = importVM.ModeOfTransport,
                    Movement = importVM.Movement,
                    Incoterm = importVM.Incoterm,
                    Factor = importVM.Factor,
                    ShipFromETDDate = importVM.ShipFromETDDate,
                    ShipToETADate = importVM.ShipToETADate,
                    ServiceType = importVM.ServiceType,
                    IsItineraryConfirmed = true,
                    TotalGrossWeightUOM = AppConstant.KILOGGRAMS,
                    TotalNetWeightUOM = AppConstant.KILOGGRAMS,
                    TotalVolumeUOM = AppConstant.CUBIC_METER,
                    OrderType = OrderType.Freight,
                    Status = StatusType.ACTIVE,
                    CreatedBy = userName,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedBy = userName,
                    UpdatedDate = DateTime.UtcNow,
                    Contacts = new List<ShipmentContactModel>(),
                    Consignments = new List<ConsignmentModel>(),
                    ShipmentLoads = new List<ShipmentLoadModel>()
                };

                if (!importVM.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase))
                {
                    newShipment.IsFCL = importVM.Movement.StartsWith("CY", StringComparison.OrdinalIgnoreCase);
                    
                }
                else
                {
                    newShipment.IsFCL = false;
                }

                CalculateShipmentTotal(importVM, ref newShipment);

                var shipFromLocation = await _csfeApiClient.GetLocationByCodeAsync(importVM.ShipFrom);
                if (shipFromLocation is not null)
                {
                    newShipment.ShipFrom = shipFromLocation.LocationDescription;
                }
                else
                {
                    importingResult.LogErrors($"{nameof(ImportShipmentViewModel.ShipFrom)} is not existing in system.");
                }

                var shipToLocation = await _csfeApiClient.GetLocationByCodeAsync(importVM.ShipTo);
                if (shipToLocation is not null)
                {
                    newShipment.ShipTo = shipToLocation.LocationDescription;
                }
                else
                {
                    importingResult.LogErrors($"{nameof(ImportShipmentViewModel.ShipTo)} is not existing in system.");
                }

                // for further uses
                var isAirShipment = newShipment.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase);
                #endregion Shipments

                #region Contacts
                await MappingContactAsync(importVM, newShipment, importingResult);
                #endregion Contacts

                #region POFulfillments
                // Get linked booking
                if (!string.IsNullOrWhiteSpace(importVM.BookingReferenceNo))
                {
                    var bookingRequest = await _poFulfillmentBookingRequestRepository.QueryAsNoTracking(
                            x => x.BookingReferenceNumber == importVM.BookingReferenceNo, includes: x => x.Include(y => y.POFulfillment)
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
                            newShipment.CargoReadyDate = poFulfillment.CargoReadyDate ?? default;
                            newShipment.BookingDate = poFulfillment.BookingDate ?? default;
                            newShipment.BookingNo = poFulfillment.Number;
                            newShipment.POFulfillmentId = poFulfillment.Id;

                            // If bulk booking, copy destination agent contact
                            if (poFulfillment.FulfillmentType == FulfillmentType.Bulk)
                            {
                                var destinationAgent = newShipment.Contacts.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.DestinationAgent, StringComparison.InvariantCultureIgnoreCase));
                                
                                if (destinationAgent != null)
                                {
                                    // Remove current destination agent before creating a new one

                                    var storedDestinationAgent = await _poFulfillmentContactRepository.QueryAsNoTracking(
                                        c => c.POFulfillmentId == poFulfillment.Id && c.OrganizationRole == destinationAgent.OrganizationRole).ToListAsync();
                                    toDeletePOFulfillmentContacts.AddRange(storedDestinationAgent);

                                    toCreatePOFulfillmentContacts.Add(new()
                                    {
                                        POFulfillmentId = poFulfillment.Id,
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
                    }
                }
                #endregion POFulfillments

                #region Consignments
                var newConsignment = new ConsignmentModel
                {
                    Sequence = 1,
                    TriangleTradeFlag = false,
                    MemoBOLFlag = false,
                    ConsignmentType = OrganizationRole.OriginAgent,
                    ShipmentLoads = new List<ShipmentLoadModel>(),
                    ShipmentLoadDetails = new List<ShipmentLoadDetailModel>(),
                    BillOfLadingConsignments = new List<BillOfLadingConsignmentModel>()
                };

                newConsignment.PopulateFromShipment(newShipment);

                newConsignment.Audit(userName);
                newShipment.Consignments.Add(newConsignment);
                #endregion Consignments

                #region Itineraries
                var itineraryList = new List<ItineraryModel>();
                var toUpdateFreightSchedulers = new List<FreightSchedulerModel>();
                var newConsignmentItineraryList = new List<ConsignmentItineraryModel>();

                if (importVM.Itineraries != null)
                {
                    foreach (var (itineraryVM, index) in importVM.Itineraries.Select((item, index) => (item, index)))
                    {
                        ItineraryModel newItinerary = new();
                        Mapper.Map(itineraryVM, newItinerary);
                        newItinerary.Status = StatusType.ACTIVE;
                        newItinerary.Audit(userName);

                        // Link shipment & consignment via ConsignmentItinerary
                        var newConsignmentItinerary = new ConsignmentItineraryModel
                        {
                            Shipment = newShipment,
                            Consignment = newConsignment,
                            CreatedBy = userName,
                            CreatedDate = DateTime.Now,
                            UpdatedBy = userName,
                            UpdatedDate = DateTime.Now,
                        };
                        newItinerary.ConsignmentItineraries = new List<ConsignmentItineraryModel>
                        {
                            newConsignmentItinerary
                        };

                        // Add to list for further links (ex: MasterBill)
                        newConsignmentItineraryList.Add(newConsignmentItinerary);

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

                        var carrier = await _csfeApiClient.GetCarrierByCodeAsync(itineraryVM.CarrierCode);
                        if (carrier != null)
                        {
                            newItinerary.CarrierName = carrier.Name;
                        }
                        var isAirItinerary = newItinerary.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase);
                        var isSeaOrAir = newItinerary.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase) || isAirItinerary;
                        // Always add new Itinerary if ModeOfTransport != Sea/Air
                        if (!isSeaOrAir)
                        {
                            itineraryList.Add(newItinerary);
                        }
                        else
                        {
                            Expression<Func<FreightSchedulerModel, bool>> freightSchedulerFilter = null;

                            if (isAirItinerary)
                            {
                                if (string.IsNullOrEmpty(importVM.MasterNo))
                                {
                                    importingResult.LogErrors($"{nameof(ImportShipmentViewModel.Itineraries)}[{index}]#Master Bill Number is required when importing Air Itinerary.");
                                }

                                freightSchedulerFilter = fs => 
                                    fs.ModeOfTransport == newItinerary.ModeOfTransport
                                    && fs.MAWB == importVM.MasterNo;
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

                            var departureEventCode = isAirItinerary ? Event.EVENT_7003 : Event.EVENT_7001;
                            var arrivalEventCode = isAirItinerary ? Event.EVENT_7004 : Event.EVENT_7002;

                            // Check if FrieghtScheduler record found with the same value of ModeofTransport = SEA, SCAC, VesselName, Voyage, LoadingPort, DischargePort
                            var existingFreightScheduler = await _freightSchedulerRepository.GetAsNoTrackingAsync(freightSchedulerFilter, includes: i => i.Include(y => y.Itineraries).ThenInclude(y => y.ConsignmentItineraries));

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
                                    MAWB = isAirItinerary ? importVM.MasterNo : null,
                                    FlightNumber = newItinerary.FlightNumber,
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

                                if (itineraryVM.IsPropertyDirty(nameof(ImportItineraryViewModel.ATDDate)) && itineraryVM.ATDDate.HasValue)
                                {
                                    freightSchedulerEventList.Add(new(departureEventCode, newFreightScheduler, NEW_EVENT));
                                }

                                if (itineraryVM.IsPropertyDirty(nameof(ImportItineraryViewModel.ATADate)) && itineraryVM.ATADate.HasValue)
                                {
                                    freightSchedulerEventList.Add(new(arrivalEventCode, newFreightScheduler, NEW_EVENT));
                                }

                                itineraryList.Add(newItinerary);
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

                                    if (itineraryVM.IsPropertyDirty(nameof(ImportItineraryViewModel.ETDDate)))
                                    {
                                        existingFreightScheduler.ETDDate = itineraryVM.ETDDate;
                                    }
                                    if (itineraryVM.IsPropertyDirty(nameof(ImportItineraryViewModel.ETADate)))
                                    {
                                        existingFreightScheduler.ETADate = itineraryVM.ETADate;
                                    }
                                    if (itineraryVM.IsPropertyDirty(nameof(ImportItineraryViewModel.ATDDate)))
                                    {
                                        var canUpdateATD = (await _freightSchedulerService.IsReadyContainerManifestAsync(existingFreightScheduler.Id)).Item1;

                                        if ((!canUpdateATD || importVM.Containers == null || !importVM.Containers.Any()) && !existingFreightScheduler.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            importingResult.LogErrors($"{nameof(ImportShipmentViewModel.Itineraries)}[{index}].{nameof(ImportItineraryViewModel.ATDDate)} can not update because the container manifest not ready for all shipments.");
                                        }
                                        else
                                        {
                                            existingFreightScheduler.ATDDate = itineraryVM.ATDDate;
                                        }
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

                                toUpdateFreightSchedulers.Add(existingFreightScheduler);

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

                                    existingItinerary.ConsignmentItineraries.Add(newConsignmentItinerary);
                                    itineraryList.Add(existingItinerary);
                                }
                                else
                                {
                                    newItinerary.ScheduleId = existingFreightScheduler.Id;
                                    itineraryList.Add(newItinerary);
                                }
                            }
                        }
                    }
                }
                #endregion Itineraries

                // Quick return if there are any mapping errors!
                if (!importingResult.Success)
                {
                    return importingResult;
                }

                // Insert into dbo.Itineraries
                await _itineraryRepository.AddRangeAsync(itineraryList.Where(x => x.Id == 0).ToArray());
                // Update dbo.Itineraries
                _itineraryRepository.UpdateRange(itineraryList.Where(x => x.Id != 0).ToArray());
                // Update dbo.FreightSchedulers
                _freightSchedulerRepository.UpdateRange(toUpdateFreightSchedulers.ToArray());

                firstItinerary = itineraryList.FirstOrDefault();

                #region Containers
                List<ContainerModel> containerList = new();
                List<ShipmentLoadModel> newShipmentLoadList = new();
                List<ConsolidationModel> consolidationList = new();
                if (isAirShipment)
                {
                    newShipmentLoadList.Add(new ShipmentLoadModel
                    {
                        ShipmentId = newShipment.Id,
                        ConsignmentId = newConsignment.Id,
                        ModeOfTransport = newShipment.ModeOfTransport,
                        CreatedBy = newShipment.CreatedBy,
                        CreatedDate = newShipment.CreatedDate,
                        UpdatedBy = newShipment.UpdatedBy,
                        UpdatedDate = newShipment.UpdatedDate,
                        IsFCL = false,
                        Shipment = newShipment, // link to Shipments
                        Consignment = newConsignment, // link to Consignments
                        BillOfLadingShipmentLoads = new List<BillOfLadingShipmentLoadModel>(),
                    });
                }
                else
                {
                    if (importVM.Containers != null)
                    {
                        foreach (var containerVM in importVM.Containers)
                        {
                            var newContainer = new ContainerModel
                            {
                                ContainerNo = containerVM.ContainerNo,
                                ContainerType = EnumExtension.GetEnumMemberValue<EquipmentType>(containerVM.ContainerType),
                                LoadingDate = containerVM.LoadingDate.HasValue ? containerVM.LoadingDate : firstItinerary?.ETDDate,
                                ShipFrom = newShipment.ShipFrom,
                                ShipFromETDDate = newShipment.ShipFromETDDate,
                                ShipTo = newShipment.ShipTo,
                                ShipToETADate = newShipment.ShipToETADate ?? default,
                                Movement = newShipment.Movement,
                                CarrierSONo = containerVM.CarrierBookingNo,
                                TotalGrossWeightUOM = AppConstant.KILOGGRAMS,
                                TotalNetWeightUOM = AppConstant.KILOGGRAMS,
                                TotalVolumeUOM = AppConstant.CUBIC_METER,
                                SealNo = containerVM.SealNo,
                                SealNo2 = containerVM.SealNo2,
                                IsFCL = newShipment.IsFCL,
                                CreatedBy = newShipment.CreatedBy,
                                CreatedDate = newShipment.CreatedDate,
                                UpdatedBy = newShipment.UpdatedBy,
                                UpdatedDate = newShipment.UpdatedDate,
                                ContainerItineraries = new List<ContainerItineraryModel>(),
                                ShipmentLoads = new List<ShipmentLoadModel>()
                            };
                            var newShipmentLoad = new ShipmentLoadModel
                            {
                                EquipmentType = newContainer.ContainerType,
                                ModeOfTransport = newShipment.ModeOfTransport,
                                CarrierBookingNo = newContainer.CarrierSONo,
                                IsFCL = newContainer.IsFCL,
                                CreatedBy = newShipment.CreatedBy,
                                CreatedDate = newShipment.CreatedDate,
                                UpdatedBy = newShipment.UpdatedBy,
                                UpdatedDate = newShipment.UpdatedDate,
                                Shipment = newShipment, // link to Shipments
                                Consignment = newConsignment, // link to Consignments
                                Container = newContainer, // link to Containers
                                ShipmentLoadDetails = new List<ShipmentLoadDetailModel>(),
                                BillOfLadingShipmentLoads = new List<BillOfLadingShipmentLoadModel>(),
                            };

                            var firstLoadDetail = importVM.CargoDetails?
                                .Where(cgd => cgd.LoadDetails != null && cgd.LoadDetails.Any())?
                                .SelectMany(cgd => cgd.LoadDetails)?
                                .FirstOrDefault(ld => ld.ContainerNo == newContainer.ContainerNo);

                            // TotalPackageUOM is the 1st item of ShipmentLoadDetails
                            if (firstLoadDetail != null)
                            {
                                newContainer.TotalPackageUOM = firstLoadDetail.PackageUOM;
                            }

                            if (newShipment.IsFCL)
                            {
                                newContainer.TotalGrossWeightUOM = newShipment.TotalGrossWeightUOM;
                                newContainer.TotalNetWeightUOM = newShipment.TotalNetWeightUOM;
                                newContainer.TotalVolumeUOM = newShipment.TotalVolumeUOM;

                                newContainer.LoadPlanRefNo = await GenerateLoadNumber(newContainer.CreatedDate);
                                containerList.Add(newContainer);
                            }
                            else
                            {
                                var newConsolidation = new ConsolidationModel
                                {
                                    OriginCFS = firstItinerary?.LoadingPort,
                                    CFSCutoffDate = containerVM.CFSCutoffDate.Value, // required if IsFCL = false
                                    ModeOfTransport = newShipment.ModeOfTransport,
                                    TotalGrossWeightUOM = AppConstant.KILOGGRAMS,
                                    TotalNetWeightUOM = AppConstant.KILOGGRAMS,
                                    TotalVolumeUOM = AppConstant.CUBIC_METER,

                                    Stage = ConsolidationStage.Confirmed,
                                    CreatedBy = newShipment.CreatedBy,
                                    CreatedDate = newShipment.CreatedDate,
                                    UpdatedBy = newShipment.UpdatedBy,
                                    UpdatedDate = newShipment.UpdatedDate,
                                    ShipmentLoads = new List<ShipmentLoadModel>()
                                };
                                newShipmentLoad.Consolidation = newConsolidation;

                                var fromDate = DateTime.Today.AddDays(-10);
                                var toDate = DateTime.Today.AddDays(10);
                                var existingContainer = await _containerRepository.Query(c => !c.IsFCL && c.ContainerNo == containerVM.ContainerNo && c.LoadingDate >= fromDate && c.LoadingDate <= toDate,
                                    null,
                                    x => x.Include(y => y.ShipmentLoads)
                                    .Include(y => y.Consolidation)
                                    .Include(y => y.ContainerItineraries)
                                    ).FirstOrDefaultAsync();

                                if (existingContainer is null)
                                {
                                    newConsolidation.PopulateFromContainer(newContainer);

                                    // link between container & consolidation
                                    newConsolidation.Container = newContainer;
                                    newContainer.Consolidation = newConsolidation;

                                    // generate load plan id
                                    newConsolidation.ConsolidationNo = await GenerateLoadPlanID(DateTime.UtcNow);
                                    // add to list for the further use.
                                    consolidationList.Add(newConsolidation);

                                    // generate load plan ref no
                                    newContainer.LoadPlanRefNo = await GenerateLoadNumber(newContainer.CreatedDate);
                                    // add to list for the further use.
                                    containerList.Add(newContainer);
                                }
                                else
                                {
                                    newShipmentLoad.EquipmentType = existingContainer.ContainerType;
                                    newShipmentLoad.CarrierBookingNo = existingContainer.CarrierSONo;

                                    // If data of each API call is different, system will store the last data imported.                        
                                    existingContainer.ContainerType = newContainer.ContainerType;
                                    if (newContainer.LoadingDate.HasValue)
                                    {
                                        existingContainer.LoadingDate = newContainer.LoadingDate.Value;
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

                                        // link between container & consolidation
                                        newConsolidation.Container = existingContainer;
                                        existingContainer.Consolidation = newConsolidation;

                                        // generate load plan id
                                        newConsolidation.ConsolidationNo = await GenerateLoadPlanID(DateTime.UtcNow);
                                        // add to list for the further use.
                                        consolidationList.Add(newConsolidation);
                                    }
                                    else
                                    {
                                        newShipmentLoad.Consolidation = existingContainer.Consolidation;
                                        // add to list for the further use.
                                        consolidationList.Add(existingContainer.Consolidation);
                                    }

                                    newShipmentLoad.Container = existingContainer;
                                    existingContainer.ShipmentLoads.Add(newShipmentLoad);

                                    // add to list for the further use.
                                    containerList.Add(existingContainer);
                                    newShipmentLoad.Container = existingContainer;
                                }
                            }

                            newShipmentLoadList.Add(newShipmentLoad);
                        }
                    }
                }
                
                #endregion Containers

                #region ContainerItineraries
                if (containerList.Count > 0 && itineraryList.Count > 0)
                {
                    containerList.ForEach(container =>
                    {
                        itineraryList.ForEach(itinerary =>
                        {
                            var isExisting = itinerary.Id != 0 && container.ContainerItineraries.Any(x => x.ItineraryId == itinerary.Id);
                            if (!isExisting)
                            {
                                container.ContainerItineraries.Add(new ContainerItineraryModel
                                {
                                    Container = container,
                                    Itinerary = itinerary,
                                    CreatedBy = newShipment.CreatedBy,
                                    CreatedDate = newShipment.CreatedDate,
                                    UpdatedBy = newShipment.UpdatedBy,
                                    UpdatedDate = newShipment.UpdatedDate,
                                });
                            }
                        });
                    });
                }
                #endregion ContainerItineraries

                #region MasterBillOfLadings
                MasterBillOfLadingModel masterBOL = null;
                if (!string.IsNullOrEmpty(importVM.MasterNo))
                {
                    masterBOL = await _masterBillOfLadingRepository.GetAsync(x => x.MasterBillOfLadingNo == importVM.MasterNo,
                        null,
                        i => i.Include(y => y.Contacts)
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
                            MasterBillOfLadingItineraries = new List<MasterBillOfLadingItineraryModel>(),
                        };
                    }
                    else
                    {
                        masterBOL.Audit(userName);
                    }

                    // If data of each API call is different, system will store the last data imported.
                    masterBOL.IsDirectMaster = importVM.IsDirectMaster ?? false;

                    if (firstItinerary != null)
                    {
                        masterBOL.SCAC = firstItinerary.SCAC;
                        masterBOL.AirlineCode = firstItinerary.AirlineCode;
                        masterBOL.VesselFlight = firstItinerary.VesselFlight;
                        masterBOL.Vessel = firstItinerary.VesselName;
                        masterBOL.Voyage = firstItinerary.Voyage;
                        masterBOL.FlightNo = firstItinerary.FlightNumber;
                        masterBOL.PlaceOfReceipt = firstItinerary.LoadingPort;
                        masterBOL.PortOfLoading = firstItinerary.LoadingPort;
                        masterBOL.PlaceOfDelivery = firstItinerary.DischargePort;
                        masterBOL.PortOfDischarge = firstItinerary.DischargePort;
                        masterBOL.PlaceOfIssue = firstItinerary.LoadingPort;
                        masterBOL.IssueDate = firstItinerary.ETDDate;
                        masterBOL.OnBoardDate = firstItinerary.ETDDate;
                    }

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

                    masterBOL.ModeOfTransport = newShipment.ModeOfTransport;
                    masterBOL.Movement = newShipment.Movement;
                    masterBOL.CarrierContractNo = newShipment.CarrierContractNo;

                    var carrier = await _csfeApiClient.GetCarrierByCodeAsync(masterBOL.ModeOfTransport == ModeOfTransport.Air ? masterBOL.AirlineCode : masterBOL.SCAC);
                    if (carrier != null)
                    {
                        masterBOL.CarrierName = carrier.Name;
                    }

                    foreach (var contact in newShipment.Contacts)
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
                            masterBOL.Contacts.Add(newMasterBOLContact);
                        }
                    }

                    // MasterBillOfLadingItineraries

                    if (itineraryList.Count > 0)
                    {
                        foreach (var itinerary in itineraryList)
                        {
                            var isExisting = itinerary.Id != 0 && masterBOL.MasterBillOfLadingItineraries.Any(x => x.ItineraryId == itinerary.Id);
                            if (!isExisting)
                            {
                                masterBOL.MasterBillOfLadingItineraries.Add(new MasterBillOfLadingItineraryModel
                                {
                                    Itinerary = itinerary,
                                    CreatedBy = userName,
                                    CreatedDate = DateTime.UtcNow,
                                    UpdatedBy = userName,
                                    UpdatedDate = DateTime.UtcNow
                                });
                            }
                        }
                    }

                    // To link master bill to Consignment
                    newConsignment.MasterBill = masterBOL;

                    // To link master bill to ConsignmentItineraries
                    if (newConsignmentItineraryList != null && newConsignmentItineraryList.Any())
                    {
                        foreach (var item in newConsignmentItineraryList)
                        {
                            item.MasterBill = masterBOL;
                        }
                    }

                    if (masterBOL.Id == 0)
                    {
                        await _masterBillOfLadingRepository.AddAsync(masterBOL);
                    }
                }
                #endregion MasterBillOfLadings

                #region BillOfLadings
                BillOfLadingModel billOfLading = null;
                if (!string.IsNullOrEmpty(importVM.HouseNo))
                {
                    billOfLading = await _billOfLadingRepository.GetAsync(x => x.BillOfLadingNo == importVM.HouseNo,
                        null,
                        i => i.Include(y => y.Contacts)
                        .Include(y => y.ShipmentBillOfLadings)
                        .Include(y => y.BillOfLadingItineraries));

                    if (billOfLading == null)
                    {
                        billOfLading = new BillOfLadingModel
                        {
                            BillOfLadingNo = importVM.HouseNo,
                            BillOfLadingType = importVM.HouseBillType,
                            JobNumber = importVM.JobNumber,
                            Incoterm = importVM.Incoterm,
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
                        // update existing record
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
                        billOfLading.Audit(userName);
                    }

                    // Calculate total bill
                    billOfLading.TotalGrossWeight += newShipment.TotalGrossWeight;
                    billOfLading.TotalGrossWeightUOM = AppConstant.KILOGGRAMS;
                    billOfLading.TotalNetWeight += newShipment.TotalNetWeight;
                    billOfLading.TotalNetWeightUOM = AppConstant.KILOGGRAMS;
                    billOfLading.TotalPackage += newShipment.TotalPackage;
                    billOfLading.TotalPackageUOM = newShipment.TotalPackageUOM;
                    billOfLading.TotalVolume += newShipment.TotalVolume;
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
                    billOfLading.ModeOfTransport = newShipment.ModeOfTransport;
                    billOfLading.ShipFrom = newShipment.ShipFrom;
                    billOfLading.ShipTo = newShipment.ShipTo;
                    billOfLading.ShipFromETDDate = newShipment.ShipFromETDDate;
                    billOfLading.ShipToETADate = newShipment.ShipToETADate ?? default;
                    billOfLading.Movement = newShipment.Movement;

                    // BillOfLadingContacts
                    foreach (var newContact in newShipment.Contacts)
                    {
                        var isExisting = billOfLading.Id != 0
                            && billOfLading.Contacts.Any(x => x.OrganizationRole == newContact.OrganizationRole && x.OrganizationId == newContact.OrganizationId);

                        if (!isExisting)
                        {
                            var newBillOfLadingContact = new BillOfLadingContactModel
                            {
                                CreatedBy = userName,
                                CreatedDate = DateTime.UtcNow,
                                UpdatedBy = userName,
                                UpdatedDate = DateTime.UtcNow,
                            };
                            newBillOfLadingContact.PopulateFromShipmentContact(newContact);
                            billOfLading.Contacts.Add(newBillOfLadingContact);
                        }
                    }

                    // ShipmentBillOfLadings
                    billOfLading.ShipmentBillOfLadings.Add(new ShipmentBillOfLadingModel
                    {
                        Shipment = newShipment,
                        CreatedBy = userName,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedBy = userName,
                        UpdatedDate = DateTime.UtcNow
                    });

                    // BillOfLadingConsignments
                    newConsignment.BillOfLadingConsignments.Add(new BillOfLadingConsignmentModel
                    {
                        BillOfLading = billOfLading,
                        Shipment = newShipment,
                        CreatedBy = userName,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedBy = userName,
                        UpdatedDate = DateTime.UtcNow
                    });

                    // BillOfLadingItineraries
                    if (itineraryList.Count > 0)
                    {
                        foreach (var itinerary in itineraryList)
                        {
                            var isExisting = itinerary.Id != 0 && billOfLading.BillOfLadingItineraries.Any(x => x.ItineraryId == itinerary.Id);
                            if (!isExisting)
                            {
                                billOfLading.BillOfLadingItineraries.Add(new BillOfLadingItineraryModel
                                {
                                    Itinerary = itinerary,
                                    CreatedBy = userName,
                                    CreatedDate = DateTime.UtcNow,
                                    UpdatedBy = userName,
                                    UpdatedDate = DateTime.UtcNow,
                                    MasterBillOfLading = masterBOL
                                });
                            }
                        }
                    }

                    // BillOfLadingShipmentLoad
                    if (newShipmentLoadList.Any())
                    {
                        foreach (var shipmentLoad in newShipmentLoadList)
                        {
                            shipmentLoad.BillOfLadingShipmentLoads.Add(new BillOfLadingShipmentLoadModel
                            {
                                BillOfLading = billOfLading,
                                Container = shipmentLoad.Container,
                                Consolidation = shipmentLoad.Consolidation,
                                MasterBillOfLading = masterBOL,
                                CreatedBy = userName,
                                CreatedDate = DateTime.UtcNow,
                                UpdatedBy = userName,
                                UpdatedDate = DateTime.UtcNow,
                            });
                        }
                    }

                    // To link house bill to Consignment
                    newConsignment.HouseBill = billOfLading;

                    if (billOfLading.Id == 0)
                    {
                        await _billOfLadingRepository.AddAsync(billOfLading);
                    }
                }
                #endregion BillOfLadings

                #region CargoDetails & LoadDetails
                if (importVM.CargoDetails != null)
                {
                    foreach (var cargoDetailVM in importVM.CargoDetails)
                    {
                        var newCargoDetailModel = new CargoDetailModel();

                        Mapper.Map(cargoDetailVM, newCargoDetailModel);

                        newCargoDetailModel.OrderType = newShipment.OrderType;
                        newCargoDetailModel.ShipmentLoadDetails = new List<ShipmentLoadDetailModel>();

                        // Load Details
                        if (cargoDetailVM.LoadDetails != null && !isAirShipment) // as business rule, there is no "Load Details" for AIR.
                        {
                            foreach (var loadDetailVM in cargoDetailVM.LoadDetails)
                            {
                                ShipmentLoadDetailModel newShipmentLoadDetail = new();

                                Mapper.Map(loadDetailVM, newShipmentLoadDetail);
                                newShipmentLoadDetail.Audit(userName);

                                var shipmentLoad = newShipmentLoadList.SingleOrDefault(x => x.Container.ContainerNo == loadDetailVM.ContainerNo);
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

                                    shipmentLoad.ShipmentLoadDetails.Add(newShipmentLoadDetail);
                                    newConsignment.ShipmentLoadDetails.Add(newShipmentLoadDetail);

                                    newShipmentLoadDetail.Shipment = newShipment; // link to shipment
                                    newShipmentLoadDetail.Container = container; // link to container

                                    newCargoDetailModel.ShipmentLoadDetails.Add(newShipmentLoadDetail);
                                }
                            }
                        }

                        newCargoDetailModel.Audit(userName);
                        newCargoDetailModel.Shipment = newShipment;
                        toCreateCargoDetails.Add(newCargoDetailModel);
                    }
                }
                #endregion CargoDetails & LoadDetails

                UnitOfWork.BeginTransaction();

                await Repository.AddAsync(newShipment);
                await UnitOfWork.SaveChangesAsync();
                /* 
                 * Warning: to "trg_CargoDetails.sql" work correctly, Please make sure that Contacts is added before CargoDetails.
                 */

                // Insert into dbo.CargoDetails (auto create dbo.ShipmentLoadDetails)
                await _cargoDetailRepository.AddRangeAsync(toCreateCargoDetails.ToArray());

                // Insert into dbo.Containers
                await _containerRepository.AddRangeAsync(containerList.Where(x => x.Id == 0).ToArray());
                // Insert into dbo.ShipmentLoads
                await _shipmentLoadRepository.AddRangeAsync(newShipmentLoadList.Where(x => x.Id == 0).ToArray());
                // Insert into dbo.Consolidations
                await _consolidationRepository.AddRangeAsync(consolidationList.Where(x => x.Id == 0).ToArray());

                // Remove dbo.POFulfillmentContacts
                _poFulfillmentContactRepository.RemoveRange(toDeletePOFulfillmentContacts.ToArray());
                // Insert into dbo.POFulfillmentContacts
                await _poFulfillmentContactRepository.AddRangeAsync(toCreatePOFulfillmentContacts.ToArray());

                await UnitOfWork.SaveChangesAsync();

                // Update some more info silent
                try
                {
                    // Update BillOfLadings.ExecutionAgentId
                    if (billOfLading != null)
                    {
                        billOfLading.ExecutionAgentId = billOfLading.Contacts.FirstOrDefault(x => x.OrganizationRole == OrganizationRole.OriginAgent)?.OrganizationId ?? 0;
                    }

                    // Update MasterBills.ExecutionAgentId
                    if (masterBOL != null)
                    {
                        masterBOL.ExecutionAgentId = masterBOL.Contacts.FirstOrDefault(x => x.OrganizationRole == OrganizationRole.OriginAgent)?.OrganizationId ?? 0;
                    }

                    // Update Consignment.ExecutionAgentId
                    if (newShipment.Consignments != null && newShipment.Consignments.Any())
                    {
                        if (newShipment.Contacts != null && newShipment.Contacts.Any())
                        {
                            var originAgent = newShipment.Contacts.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.OriginAgent);
                            if (originAgent != null)
                            {
                                string executionAgentName = null;
                                if (originAgent.OrganizationId != 0)
                                {
                                    var originAgentOrg = await _csfeApiClient.GetOrganizationByIdAsync(originAgent.OrganizationId);
                                    executionAgentName = originAgentOrg?.Name;
                                }
                                foreach (var item in newShipment.Consignments)
                                {
                                    item.ExecutionAgentId = originAgent.OrganizationId;
                                    item.ExecutionAgentName = executionAgentName;
                                }
                            }
                        }
                    }

                    await UnitOfWork.SaveChangesAsync();
                }
                catch (Exception)
                {
                }

                #region Trigger/Update/Delete Arrival/Departure event
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
                        await _activityService.CreateAsync(newActivityVM);
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
                                await _activityService.UpdateAsync(activityVM, activityVM.Id);
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

                                await _activityService.CreateAsync(newActivityVM);
                            }
                        }
                        else
                        {
                            if (activityInSystem != null)
                            {
                                await _activityService.DeleteAsync(activityInSystem.Id);
                            }
                        }
                    }
                }
                #endregion Trigger/Update/Delete Arrival/Departure event

                if (broadcastFreightScheduleIdList.Count > 0)
                {
                    await _freightSchedulerService.BroadcastFreightScheduleUpdatesAsync(broadcastFreightScheduleIdList.Select(x => x.Id), Schedulers.UpdatedViaFreightSchedulerAPI, false, userName);
                }

                UnitOfWork.CommitTransaction();

                importingResult.LogSuccess("Id", $"{newShipment.Id}");
                importingResult.LogSuccess("Number", $"{newShipment.ShipmentNo}");
                importingResult.LogSuccess("Url", $"{_appConfig.ClientUrl}/shipments/{newShipment.Id}");

                return importingResult;
            case ImportShipmentStatus.U:

                return await UpdateFreightShipmentAsync(importVM, userName);

            case ImportShipmentStatus.D:

                var shipmentVM = await CancelShipmentAsync(importVM.ShipmentNo, userName);

                importingResult.LogSuccess("Id", $"{shipmentVM.Id}");
                importingResult.LogSuccess("Number", $"{shipmentVM.ShipmentNo}");
                importingResult.LogSuccess("Url", $"{_appConfig.ClientUrl}/shipments/{shipmentVM.Id}");

                return importingResult;
            default:
                break;
        }

        return importingResult;
    }

    /// <summary>
    /// To cancel Shipment via Import API.
    /// </summary>
    /// <param name="shipmentNo"></param>
    /// <param name="userName"></param>
    /// <returns></returns>
    /// <exception cref="AppEntityNotFoundException"></exception>
    private async Task<ShipmentViewModel> CancelShipmentAsync(string shipmentNo, string userName)
    {
        var toDeleteShipment = await Repository.GetAsync(x => x.ShipmentNo == shipmentNo, includes: x 
            => x.Include(s => s.Consignments)
            .Include(s => s.POFulfillment)
            .Include(s => s.ShipmentLoads).ThenInclude(sl => sl.ShipmentLoadDetails)
            .Include(s => s.ShipmentLoads).ThenInclude(sl => sl.Consolidation));

        if (toDeleteShipment == null)
        {
            throw new AppEntityNotFoundException($"Object with the number {string.Join(", ", shipmentNo)} not found!");
        }

        if (toDeleteShipment.POFulfillmentId.HasValue)
        {
            var associatedBooking = await _poFulfillmentRepository.GetAsNoTrackingAsync(p => p.Id == toDeleteShipment.POFulfillmentId);

            if (associatedBooking.Stage == POFulfillmentStage.Closed)
            {
                throw new AppValidationException("Cannot cancel Shipment! Associated Booking is closed.");
            }
        }

        // Validate: Not allow to cancel shipment as container is already loaded.
        var isAlreadyLoaded = toDeleteShipment.ShipmentLoads.Any(sl => sl.ShipmentLoadDetails?.Count > 0);
        if (isAlreadyLoaded)
        {
            throw new AppValidationException("Not allow to cancel the shipment because its container is already loaded.");
        }

        UnitOfWork.BeginTransaction();

        toDeleteShipment.IsItineraryConfirmed = false;

        if (toDeleteShipment.Consignments != null && toDeleteShipment.Consignments.Any())
        {
            foreach (var consigment in toDeleteShipment.Consignments)
            {
                consigment.Status = StatusType.INACTIVE;
                consigment.Audit(userName);
            }
        }

        POFulfillmentBookingRequestModel bookingRequest = null;
        if (toDeleteShipment.POFulfillmentId.HasValue)
        {
            if (toDeleteShipment.POFulfillment.FulfillmentType == FulfillmentType.PO)
            {
                POFulfillmentService.ReleaseQuantityOnPOLineItems(toDeleteShipment.POFulfillmentId.Value);
            }
            bookingRequest = await UpdateBookingToDraft(toDeleteShipment.POFulfillmentId.Value, userName);
        }

        #region Itineraries
        var toDeleteItineraries = new List<ItineraryModel>();
        var toUnlinkItineraries = new List<ItineraryModel>();

        var billOfLadingItineraries = new List<BillOfLadingItineraryModel>();

        // List of itineraries of deleting shipment.
        var itineraries = await _itineraryRepository.Query(i => i.ConsignmentItineraries.Any(ci => ci.ShipmentId == toDeleteShipment.Id), includes: x => x.Include(y => y.ConsignmentItineraries)
            .Include(y => y.ContainerItineraries)
            .Include(y => y.MasterBillOfLadingItineraries)
            .Include(y => y.BillOfLadingItineraries)).ToListAsync();

        if (itineraries != null && itineraries.Any())
        {
            var itineraryIds = itineraries.Select(x => x.Id).ToList();
            var inSharingItineraryIds = await _consignmentItineraryRepository.Query(ci => itineraryIds.Contains(ci.ItineraryId)).Select(x => new { x.ItineraryId, x.ShipmentId }).Distinct().ToListAsync();

            // Itineraries not sharing with other shipments
            toDeleteItineraries = itineraries.Where(x => inSharingItineraryIds.Count(y => y.ItineraryId == x.Id) == 1).ToList();
            // Itineraries sharing with other shipments
            toUnlinkItineraries = itineraries.Where(x => inSharingItineraryIds.Count(y => y.ItineraryId == x.Id) > 1).ToList();

            billOfLadingItineraries = itineraries.SelectMany(i => i.BillOfLadingItineraries).ToList();
        }

        foreach (var item in toDeleteItineraries)
        {
            // Remove appropriate linked tables
            item.ConsignmentItineraries.Clear();
            item.ConsignmentItineraries.Clear();
            item.ContainerItineraries.Clear();
            item.MasterBillOfLadingItineraries.Clear();
            item.BillOfLadingItineraries.Clear();
        }
        _itineraryRepository.RemoveRange(toDeleteItineraries.ToArray());

        foreach (var item in toUnlinkItineraries)
        {
            // Remove appropriate Consignment Itineraries
            item.ConsignmentItineraries = item.ConsignmentItineraries.Where(x => x.ShipmentId != toDeleteShipment.Id).ToList();
        }
        await UnitOfWork.SaveChangesAsync();
        #endregion Itineraries

        #region Containers
        var toDeleteContainers = new List<ContainerModel>();
        var toUnlinkContainers = new List<ContainerModel>();

        var toDeleteConsolidations = new List<ConsolidationModel>();

        // Get containers stored in database = CY + CFS
        // CY container will be removed
        var containers = await _containerRepository.Query(x => x.ShipmentLoads.Any(x => x.ShipmentId == toDeleteShipment.Id),
                                                                    includes: x => x.Include(y => y.ContainerItineraries).Include(y => y.Consolidation)
                                                             ).ToListAsync();

        // Get CargoDetails stored in database
        var cargoDetails = await _cargoDetailRepository.Query(x => x.ShipmentId == toDeleteShipment.Id).ToListAsync();

        // Sum up ShipmentLoadDetails for further unlink calculation.
        var totalGrossWeight = cargoDetails?.Sum(x => x.GrossWeight) ?? 0;
        var totalNetWeight = cargoDetails?.Sum(x => x.NetWeight) ?? 0;
        var totalPackage = (int)(cargoDetails?.Sum(x => x.Package) ?? 0);
        var totalVolumn = cargoDetails?.Sum(x => x.Volume) ?? 0;

        if (containers != null && containers.Any())
        {
            // Need to check CFS containers in sharing
            var cfsContainers = containers.Where(x => !x.IsFCL).ToList();
            var cfsContainerIds = cfsContainers.Select(x => x.Id).ToList();

            var inSharingContainerIds = await _shipmentLoadRepository.Query(sl => sl.ContainerId != null && cfsContainerIds.Contains(sl.ContainerId.Value)).Select(x => new { x.ContainerId, x.ShipmentId }).Distinct().ToListAsync();

            // Remove CY containers
            toDeleteContainers = containers.Where(x => x.IsFCL).ToList();

            // Containers not sharing with other shipments
            toDeleteContainers.AddRange(cfsContainers.Where(x => inSharingContainerIds.Count(y => y.ContainerId == x.Id) == 1).ToList());
            // Delete Consolidations
            toDeleteConsolidations = toDeleteContainers.Where(x => x.Consolidation != null).Select(x => x.Consolidation).ToList();

            // Containers sharing with other shipments
            toUnlinkContainers = cfsContainers.Where(x => inSharingContainerIds.Count(y => y.ContainerId == x.Id) > 1).ToList();

            foreach (var item in toDeleteContainers)
            {
                item.ContainerItineraries.Clear();
            }
            _containerRepository.RemoveRange(toDeleteContainers.ToArray());
            _consolidationRepository.RemoveRange(toDeleteConsolidations.ToArray());

            foreach (var item in toUnlinkContainers)
            {
                item.TotalGrossWeight -= totalGrossWeight;
                item.TotalNetWeight -= totalNetWeight;
                item.TotalPackage -= totalPackage;
                item.TotalVolume -= totalVolumn;

                // Recalculate consolidation
                item.Consolidation.TotalGrossWeight -= totalGrossWeight;
                item.Consolidation.TotalNetWeight -= totalNetWeight;
                item.Consolidation.TotalPackage -= totalPackage;
                item.Consolidation.TotalVolume -= totalVolumn;
            }
        }
        #endregion Containers

        #region BillOfLadings
        var toDeleteBillOfLadings = new List<BillOfLadingModel>();
        var toUnlinkBillOfLadings = new List<BillOfLadingModel>();

        var billOfLadings = await _billOfLadingRepository.Query(bl => bl.ShipmentBillOfLadings.Any(sbl => sbl.ShipmentId == toDeleteShipment.Id),
            includes: x => x.Include(y => y.BillOfLadingConsignments).Include(y => y.BillOfLadingItineraries).Include(y => y.ShipmentBillOfLadings).Include(y => y.BillOfLadingShipmentLoads)).ToListAsync();

        if (billOfLadings != null && billOfLadings.Any())
        {
            var billOfLadingIds = billOfLadings.Select(bl => bl.Id).ToList();
            var inSharingBillOfLadingIds = await _shipmentBillOfLadingRepository.Query(sbl => billOfLadingIds.Contains(sbl.BillOfLadingId)).Select(x => new { x.BillOfLadingId, x.ShipmentId }).Distinct().ToListAsync();

            // Bill of ladings not sharing with other shipments
            toDeleteBillOfLadings = billOfLadings.Where(x => inSharingBillOfLadingIds.Count(y => y.BillOfLadingId == x.Id) == 1).ToList();

            // Bill of ladings sharing with other shipments
            toUnlinkBillOfLadings = billOfLadings.Where(x => inSharingBillOfLadingIds.Count(y => y.BillOfLadingId == x.Id) > 1).ToList();
        }

        foreach (var item in toDeleteBillOfLadings)
        {
            item.BillOfLadingConsignments.Clear();
            item.BillOfLadingItineraries.Clear();
            item.ShipmentBillOfLadings.Clear();
            item.BillOfLadingShipmentLoads.Clear();
        }
        _billOfLadingRepository.RemoveRange(toDeleteBillOfLadings.ToArray());

        foreach (var item in toUnlinkBillOfLadings)
        {
            item.BillOfLadingConsignments = item.BillOfLadingConsignments.Where(blc => blc.ShipmentId != toDeleteShipment.Id).ToList();
            item.ShipmentBillOfLadings = item.ShipmentBillOfLadings.Where(sbl => sbl.ShipmentId != toDeleteShipment.Id).ToList();
            // dbo.[BillOfLadingShipmentLoads] -> no need to unlink as it will all be deleted by shipment

            // Remove out of Consignment
            foreach (var consignment in toDeleteShipment.Consignments)
            {
                if (consignment.HouseBillId.HasValue && consignment.HouseBillId.Value == item.Id)
                {
                    consignment.HouseBill = null;
                }
            }
            // Recalculate bill of lading total
            item.TotalGrossWeight -= totalGrossWeight;
            item.TotalNetWeight -= totalNetWeight;
            item.TotalPackage -= totalPackage;
            item.TotalVolume -= totalVolumn;
        }
        #endregion BillOfLadings

        #region MasterBills
        var toDeleteMasterBillOfLadings = new List<MasterBillOfLadingModel>();
        var toUnlinkMasterBillOfLadings = new List<MasterBillOfLadingModel>();

        var masterBillOfLadings = await _masterBillOfLadingRepository.Query(mb => mb.Consignments.Any(csm => csm.ShipmentId == toDeleteShipment.Id), includes: x => x.Include(y => y.MasterBillOfLadingItineraries)).ToListAsync();

        if (masterBillOfLadings != null && masterBillOfLadings.Any())
        {
            var masterBillOfLadingIds = masterBillOfLadings.Select(bl => bl.Id).ToList();
            var inSharingMasterBillOfLadingIds = await _consignmentRepository.Query(csm => csm.MasterBillId != null && masterBillOfLadingIds.Contains(csm.MasterBillId.Value)).Select(x => new { x.MasterBillId, x.ShipmentId }).Distinct().ToListAsync();

            // Master bills not sharing with other shipments
            toDeleteMasterBillOfLadings = masterBillOfLadings.Where(x => inSharingMasterBillOfLadingIds.Count(y => y.MasterBillId == x.Id) == 1).ToList();

            // Master bills sharing with other shipments
            toUnlinkMasterBillOfLadings = masterBillOfLadings.Where(x => inSharingMasterBillOfLadingIds.Count(y => y.MasterBillId == x.Id) > 1).ToList();
        }
        toDeleteMasterBillOfLadings.ForEach(mb => mb.MasterBillOfLadingItineraries.Clear());
        _masterBillOfLadingRepository.RemoveRange(toDeleteMasterBillOfLadings.ToArray());

        // Unlink Bill of lading out of the cancelling Shipment.
        foreach (var item in toUnlinkMasterBillOfLadings)
        {
            // Unlink out of Consignments
            foreach (var consignment in toDeleteShipment.Consignments)
            {
                if (consignment.MasterBillId.HasValue && consignment.MasterBillId.Value == item.Id)
                {
                    consignment.MasterBill = null;
                }
            }

            // dbo.[ConsignmentItineraries] -> no need to unlink as it will all be deleted by unlinking/deleting itineraries
            // dbo.[BillOfLadingShipmentLoads] -> no need to unlink as it will all be deleted by shipment

            // Unlink out of BillOfLadingItineraries
            foreach (var billOfLadingItinerary in billOfLadingItineraries)
            {
                if (billOfLadingItinerary.MasterBillOfLadingId.HasValue && billOfLadingItinerary.MasterBillOfLadingId.Value == item.Id)
                {
                    billOfLadingItinerary.MasterBillOfLading = null;
                }
            }
        }
        #endregion MasterBills

        #region ShipmentLoads
        var toDeleteShipmentLoads = new List<ShipmentLoadModel>();
        if (toDeleteShipment.ShipmentLoads != null && toDeleteShipment.ShipmentLoads.Any())
        {
            toDeleteShipmentLoads = toDeleteShipment.ShipmentLoads.ToList();
        }
        _shipmentLoadRepository.RemoveRange(toDeleteShipmentLoads.ToArray());
        #endregion ShipmentLoads

        toDeleteShipment.SetCancelledStatus();
        toDeleteShipment.Audit(userName);

        await UnitOfWork.SaveChangesAsync();

        await Trigger1070EventAsync(toDeleteShipment, userName);

        UnitOfWork.CommitTransaction();

        return Mapper.Map<ShipmentViewModel>(toDeleteShipment);
    }

    private static void CalculateShipmentTotal(ImportShipmentViewModel importVM, ref ShipmentModel shipment)
    {
        if (shipment is null)
        {
            return;
        }
        shipment.TotalGrossWeight = importVM.CargoDetails?.Sum(x => x.GrossWeight) ?? shipment.TotalGrossWeight;
        shipment.TotalNetWeight = importVM.CargoDetails?.Sum(x => x.NetWeight) ?? shipment.TotalNetWeight;
        shipment.TotalPackage = importVM.CargoDetails?.Sum(x => x.Package) ?? shipment.TotalPackage;
        shipment.TotalPackageUOM = importVM.CargoDetails?.First().PackageUOM ?? shipment.TotalPackageUOM;
        shipment.TotalVolume = importVM.CargoDetails?.Sum(x => x.Volume) ?? shipment.TotalVolume;
        shipment.TotalUnit = importVM.CargoDetails?.Sum(x => x.Unit) ?? shipment.TotalUnit;
        shipment.TotalUnitUOM = importVM.CargoDetails?.First().UnitUOM ?? shipment.TotalUnitUOM;
    }

    private async Task MappingContactAsync(ImportShipmentViewModel importVM, ShipmentModel shipment, ImportingShipmentResultViewModel importingResult)
    {
        if (importVM.Contacts == null || !importVM.Contacts.Any())
        {
            return;
        }

        var organizationCodes = importVM.Contacts
            .Select(c => c.OrganizationCode)
            .Where(r => r != "0" && !string.IsNullOrWhiteSpace(r))
            .Distinct()
            .ToArray();

        var organizations = await _csfeApiClient.GetOrganizationsByEdisonCompanyIdCodesAsync(organizationCodes);

        foreach (var (contactVM, index) in importVM.Contacts.Select((item, index) => (item, index)))
        {
            var newContact = new ShipmentContactModel();

            Mapper.Map(contactVM, newContact);

            if (!string.IsNullOrWhiteSpace(contactVM.OrganizationCode) && contactVM.OrganizationCode != "0")
            {
                var organization = organizations.FirstOrDefault(org => org.EdisonCompanyCodeId == contactVM.OrganizationCode);

                if (organization is not null)
                {
                    newContact.OrganizationId = organization.Id;

                    if (string.IsNullOrWhiteSpace(newContact.CompanyName))
                    {
                        newContact.CompanyName = organization.Name;
                    }

                    if (string.IsNullOrWhiteSpace(newContact.Address))
                    {
                        newContact.Address = CompanyAddressLinesResolver.ConcatenateCompanyAddressLines(organization.Address, organization.AddressLine2, organization.AddressLine3, organization.AddressLine4);
                    }

                    if (string.IsNullOrWhiteSpace(newContact.ContactName))
                    {
                        newContact.ContactName = organization.ContactName;
                    }

                    if (string.IsNullOrWhiteSpace(newContact.ContactNumber))
                    {
                        newContact.ContactNumber = organization.ContactNumber;
                    }

                    if (string.IsNullOrWhiteSpace(newContact.ContactEmail))
                    {
                        newContact.ContactEmail = organization.ContactEmail;
                    }
                }
                else
                {
                    importingResult.LogErrors($"{nameof(ImportShipmentViewModel.Contacts)}[{index}].{nameof(ImportShipmentContactViewModel.OrganizationCode)} is not existing in system.");
                }
            }
            newContact.Audit(shipment.CreatedBy);
            shipment.Contacts.Add(newContact);
        }
    }

    public async Task WriteImportingResultLogAsync(ImportingShipmentResultViewModel importingResult, ImportShipmentViewModel importVM, string profile, string userName)
    {
        var logModel = new IntegrationLogModel
        {
            APIName = "Import Freight Shipment",
            APIMessage = $"CreatedBy: {userName} \nCreatedDate: {DateTime.UtcNow} \nStatus: {importVM.Status?.ToString() ?? string.Empty}",
            EDIMessageRef = string.Empty,
            EDIMessageType = string.Empty,
            PostingDate = DateTime.UtcNow,
            Profile = profile,
            Status = importingResult.Success ? IntegrationStatus.Succeed : IntegrationStatus.Failed,
            Remark = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} GMT",
            Response = JsonConvert.SerializeObject(importingResult, Formatting.Indented)
        };
        await _integrationLogRepository.AddAsync(logModel);
        await UnitOfWork.SaveChangesAsync();
    }

    private async Task<string> GenerateLoadNumber(DateTime createdDate)
    {
        var nextSequenceValue = await _poFulfillmentRepository.GetNextPOFFLoadSequenceValueAsync();
        return $"LP{createdDate.ToString("yyMM")}{nextSequenceValue}";
    }

    private async Task<string> GenerateLoadPlanID(DateTime createdDate)
    {
        var nextSequenceValue = await _consolidationRepository.GetNextLoadPlanIDSequenceValueAsync();
        return $"LOAD{createdDate.ToString("yyMM")}{nextSequenceValue}";
    }
}