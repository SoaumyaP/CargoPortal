using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.ApplicationBackgroundJob;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Application.Translations.Helpers;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.POFulfillment.Services
{
    public partial class POFulfillmentService
    {
        public async Task<ImportBookingResult> ImportBookingAsync(ImportBookingViewModel importVM, string userName)
        {
            List<System.ComponentModel.DataAnnotations.ValidationResult> importingResult = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

            // Cancel.
            if (importVM.Status == POFulfillmentStatus.Inactive)
            {
                long? cancelingPOFFId = 0;
                string cancelingBookingRefNumber = "";

                if (!string.IsNullOrWhiteSpace(importVM.SONumber))
                {
                    var firstSONumber = importVM.SONumber.Split(",")[0];

                    var poffBookingRequests = await _poFulfillmentBookingRequestRepository.QueryAsNoTracking(
                        x => x.SONumber.Contains(firstSONumber) && x.Status == POFulfillmentBookingRequestStatus.Active).ToListAsync();

                    var matchedPOFFBookingRequest = poffBookingRequests?.Where(x => x.SONumber.Split(",").Contains(firstSONumber)).FirstOrDefault();
                    cancelingPOFFId = matchedPOFFBookingRequest?.POFulfillmentId;
                    cancelingBookingRefNumber = matchedPOFFBookingRequest?.BookingReferenceNumber;
                }
                if (cancelingPOFFId == 0 || cancelingPOFFId == null)
                {
                    var error = new System.ComponentModel.DataAnnotations.ValidationResult($"Booking not found!",
                                new[] { $"Booking" });
                    importingResult.Add(error);

                    return new ImportBookingResult
                    {
                        Success = false,
                        Result = importingResult
                    };
                }

                var hasLinkedShipment = await _shipmentRepository.AnyAsync(x => x.POFulfillmentId == cancelingPOFFId && x.Status == StatusType.ACTIVE);
                if (hasLinkedShipment)
                {
                    var error = new System.ComponentModel.DataAnnotations.ValidationResult($"Booking with linked shipment are not allow to cancel.",
                                new[] { $"Booking" });
                    importingResult.Add(error);

                    return new ImportBookingResult
                    {
                        Success = false,
                        Result = importingResult
                    };
                }

                UnitOfWork.BeginTransaction();

                Func<IQueryable<POFulfillmentModel>, IQueryable<POFulfillmentModel>> includeProperties = x
                    => x.Include(m => m.Contacts)
                    .Include(m => m.Loads)
                    .Include(m => m.Orders)
                    .Include(m => m.BookingRequests)
                    .Include(m => m.Itineraries)
                    .Include(m => m.BuyerApprovals)
                    .Include(m => m.Shipments)
                    .ThenInclude(i => i.Consignments);

                var poff = await Repository.GetAsync(x => x.Id == cancelingPOFFId, null, includeProperties);

                if (poff == null)
                {
                    var error = new System.ComponentModel.DataAnnotations.ValidationResult($"Booking not found!",
                                new[] { $"Booking" });

                    importingResult.Add(error);

                    return new ImportBookingResult
                    {
                        Success = false,
                        Result = importingResult
                    };
                }

                poff.Status = POFulfillmentStatus.Inactive;
                poff.IsRejected = false;
                poff.BookingDate = null;

                // Set status of POFulfillmentLoads = Inactive
                foreach (var load in poff.Loads)
                {
                    load.Status = POFulfillmentLoadStatus.Inactive;
                }

                // Set status of POFulfillmentOrders = Inactive
                foreach (var order in poff.Orders)
                {
                    order.Status = POFulfillmentOrderStatus.Inactive;
                }

                // Set status of POFulfillmentItineraries = Inactive
                foreach (var itinerary in poff.Itineraries)
                {
                    itinerary.Status = POFulfillmentItinerayStatus.Inactive;
                }

                ReleaseQuantityOnPOLineItems(poff.Id);

                var event1057 = new ActivityViewModel()
                {
                    ActivityCode = Event.EVENT_1057,
                    POFulfillmentId = poff.Id,
                    ActivityDate = DateTime.UtcNow,
                    CreatedBy = userName,
                    Remark = string.Empty
                };
                await _activityService.TriggerAnEvent(event1057);

                // Cancel current Booking Request
                var bookingRequest = poff.BookingRequests.SingleOrDefault(br => br.Status == POFulfillmentBookingRequestStatus.Active);
                if (bookingRequest != null)
                {
                    bookingRequest.Status = POFulfillmentBookingRequestStatus.Inactive;
                }

                Repository.Update(poff);

                // Update PO stage
                await UpdatePurchaseOrderStageByPOFFAsync(poff);

                // When user cancel a FB Request fulfillment which has a pending approval associated,
                // update stage of buyer approval to Cancel as well.
                var buyerApproval = await _buyerApprovalRepository.GetAsync(x => x.POFulfillmentId == poff.Id && x.Stage == BuyerApprovalStage.Pending);
                if (buyerApproval != null)
                {
                    buyerApproval.Stage = BuyerApprovalStage.Cancel;
                    _buyerApprovalRepository.Update(buyerApproval);
                }

                // Remove all POAdhocChanges for the Booking
                DeletePurchaseOrderAdhocChanges(0, poff.Id, 0);

                await UnitOfWork.SaveChangesAsync();
                UnitOfWork.CommitTransaction();

                return new ImportBookingResult
                {
                    Success = true,
                    BookingId = poff.Id,
                    BookingRefNumber = cancelingBookingRefNumber
                };
            }
            // Import
            else
            {
                long? bookingId = 0;
                POFulfillmentBookingRequestModel matchedPOFFBookingRequest = null;
                if (!string.IsNullOrWhiteSpace(importVM.SONumber))
                {
                    var firstSONumber = importVM.SONumber.Split(",")[0];

                    var poffBookingRequests = await _poFulfillmentBookingRequestRepository.QueryAsNoTracking(
                     x => x.SONumber.Contains(firstSONumber)).ToListAsync();

                    matchedPOFFBookingRequest = poffBookingRequests?.Where(x => x.SONumber.Split(",").Contains(firstSONumber)).FirstOrDefault();
                    bookingId = matchedPOFFBookingRequest?.POFulfillmentId;
                }

                // Add new booking
                if (bookingId == 0 || bookingId == null)
                {
                    UnitOfWork.BeginTransaction();
                    #region POFulfillments
                    var poFulfillment = new POFulfillmentModel
                    {
                        Owner = importVM.Owner,
                        Status = importVM.Status,
                        CargoReadyDate = importVM.CargoReadyDate,
                        ExpectedShipDate = importVM.ExpectedShipDate,
                        ExpectedDeliveryDate = importVM.ExpectedDeliveryDate,
                        Incoterm = importVM.Incoterm,
                        ModeOfTransport = importVM.ModeOfTransport,
                        LogisticsService = (LogisticsServiceType)importVM.LogisticsService,
                        MovementType = (MovementType)importVM.MovementType,
                        Remarks = importVM.Remarks,
                        IsContainDangerousGoods = Convert.ToBoolean((int)importVM.IsContainDangerousGoods),
                        IsShipperPickup = Convert.ToBoolean((int)importVM.IsShipperPickup),
                        IsNotifyPartyAsConsignee = Convert.ToBoolean((int)importVM.IsNotifyPartyAsConsignee),
                        IsBatteryOrChemical = Convert.ToBoolean((int)importVM.IsBatteryOrChemical),
                        IsCIQOrFumigation = Convert.ToBoolean((int)importVM.IsCIQOrFumigation),
                        IsExportLicence = Convert.ToBoolean((int)importVM.IsExportLicence),
                        BookingDate = importVM.BookingDate,
                        VesselName = importVM.VesselName,
                        VoyageNo = importVM.VoyageNo,
                        CYClosingDate = importVM.CYClosingDate,
                        CYEmptyPickupTerminalCode = importVM.CYEmptyPickupTerminalCode,
                        CYEmptyPickupTerminalDescription = importVM.CYEmptyPickupTerminalDescription,
                        CFSClosingDate = importVM.CFSClosingDate,
                        CFSWarehouseCode = importVM.CFSWarehouseCode,
                        CFSWarehouseDescription = importVM.CFSWarehouseDescription,
                        CreatedBy = importVM.CreatedBy,
                        CreatedDate = importVM.CreatedDate,
                        UpdatedBy = importVM.UpdatedBy,
                        UpdatedDate = importVM.UpdatedDate
                    };

                    var locations = (await _csfeApiClient.GetLocationsByCodesAsync(new List<string> {
                    importVM.ShipFrom,
                    importVM.ShipTo,
                    importVM.DeliveryPort,
                    importVM.ReceiptPort
                })).ToList();

                    var shipFrom = locations.FirstOrDefault(x => x.Name == importVM.ShipFrom);
                    var shipTo = locations.FirstOrDefault(x => x.Name == importVM.ShipTo);
                    var deliveryPort = locations.FirstOrDefault(x => x.Name == importVM.DeliveryPort);
                    var receiptPort = locations.FirstOrDefault(x => x.Name == importVM.ReceiptPort);

                    if (shipFrom != null)
                    {
                        poFulfillment.ShipFrom = shipFrom.Id;
                        poFulfillment.ShipFromName = shipFrom.LocationDescription;
                    }

                    if (shipTo != null)
                    {
                        poFulfillment.ShipTo = shipTo.Id;
                        poFulfillment.ShipToName = shipTo.LocationDescription;
                    }

                    if (deliveryPort != null)
                    {
                        poFulfillment.DeliveryPortId = deliveryPort.Id;
                        poFulfillment.DeliveryPort = deliveryPort.LocationDescription;
                    }

                    if (receiptPort != null)
                    {
                        poFulfillment.ReceiptPortId = receiptPort.Id;
                        poFulfillment.ReceiptPort = receiptPort.LocationDescription;
                    }

                    if (!string.IsNullOrWhiteSpace(importVM.PreferredCarrier))
                    {
                        var preferredCarrier = await _csfeApiClient.GetCarrierByCodeAsync(importVM.PreferredCarrier);
                        poFulfillment.PreferredCarrier = preferredCarrier.Id;
                    }
                    #endregion POFulfillments

                    #region POFulfillmentContacts
                    poFulfillment.Contacts = new List<POFulfillmentContactModel>();

                    var contactOrgCodes = importVM.Contacts
                        .Select(x => x.OrganizationCode)
                        .Where(x => x != "0");
                    var organizations = await _csfeApiClient.GetOrganizationsByCodesAsync(contactOrgCodes);
                    foreach (var item in importVM.Contacts)
                    {
                        var poffContact = new POFulfillmentContactModel();
                        Mapper.Map(item, poffContact);

                        if (item.OrganizationCode != "0")
                        {
                            var organization = organizations.FirstOrDefault(x => x.Code == item.OrganizationCode);
                            if (organization != null)
                            {
                                poffContact.OrganizationId = organization.Id;
                                poffContact.CompanyName = organization.Name;
                                poffContact.Address = organization.Address;
                                poffContact.AddressLine2 = organization.AddressLine2;
                                poffContact.AddressLine3 = organization.AddressLine3;
                                poffContact.AddressLine4 = organization.AddressLine4;
                                poffContact.ContactName = organization.ContactName;
                                poffContact.ContactNumber = organization.ContactNumber;
                                poffContact.ContactEmail = organization.ContactEmail;
                            }
                        }
                        poffContact.CreatedBy = poFulfillment.CreatedBy;
                        poffContact.CreatedDate = poFulfillment.CreatedDate;
                        poffContact.UpdatedBy = poFulfillment.UpdatedBy;
                        poffContact.UpdatedDate = poFulfillment.UpdatedDate;

                        poFulfillment.Contacts.Add(poffContact);
                    }
                    #endregion POFulfillmentContacts

                    #region POFulfillmentLoads
                    poFulfillment.Loads = new List<POFulfillmentLoadModel>();
                    foreach (var item in importVM.Loads)
                    {
                        var poffLoad = Mapper.Map<POFulfillmentLoadModel>(item);
                        poffLoad.Status = POFulfillmentLoadStatus.Active;
                        poffLoad.CreatedBy = poFulfillment.CreatedBy;
                        poffLoad.CreatedDate = poFulfillment.CreatedDate;
                        poffLoad.UpdatedBy = poFulfillment.UpdatedBy;
                        poffLoad.UpdatedDate = poFulfillment.UpdatedDate;

                        poFulfillment.Loads.Add(poffLoad);
                    }
                    #endregion

                    #region CustomerPOs
                    poFulfillment.Orders = new List<POFulfillmentOrderModel>();
                    var principalContact = poFulfillment.Contacts.First(x => x.OrganizationRole == OrganizationRole.Principal);

                    var poNumbers = importVM.CustomerPOs.Select(x => x.CustomerPONumber);
                    var pos = await _purchaseOrderRepository.Query(
                        x => poNumbers.Contains(x.PONumber)
                        && x.Contacts.Any(c => c.OrganizationRole == OrganizationRole.Principal && c.OrganizationId == principalContact.OrganizationId), null, i => i.Include(y => y.LineItems)).ToListAsync();

                    foreach (var (item, index) in importVM.CustomerPOs.Select((item, index) => (item, index)))
                    {
                        var po = pos.FirstOrDefault(x => x.PONumber == item.CustomerPONumber);
                        if (po != null)
                        {
                            var poffOrder = Mapper.Map<POFulfillmentOrderModel>(item);
                            poffOrder.PurchaseOrderId = po.Id;
                            poffOrder.Status = POFulfillmentOrderStatus.Active;

                            var lineItem = po.LineItems.FirstOrDefault(x => x.ProductCode == poffOrder.ProductCode);
                            if (lineItem != null)
                            {
                                poffOrder.POLineItemId = lineItem.Id;
                                poffOrder.ProductName = lineItem.ProductName;

                                // calculate balance qty.
                                poffOrder.OrderedUnitQty = lineItem.OrderedUnitQty;
                                poffOrder.BalanceUnitQty = lineItem.OrderedUnitQty - poffOrder.FulfillmentUnitQty;

                                poffOrder.CreatedBy = poFulfillment.CreatedBy;
                                poffOrder.CreatedDate = poFulfillment.CreatedDate;
                                poffOrder.UpdatedBy = poFulfillment.UpdatedBy;
                                poffOrder.UpdatedDate = poFulfillment.UpdatedDate;

                                poFulfillment.Orders.Add(poffOrder);

                                // If the current stage = Released, Update PO stage = Booking Confirmed.
                                if (po.Stage == POStageType.Released)
                                {
                                    po.Stage = POStageType.ForwarderBookingConfirmed;
                                }
                            }
                            else
                            {
                                var error = new System.ComponentModel.DataAnnotations.ValidationResult($"{nameof(ImportBookingOrderViewModel.ProductCode)} is not existing.",
                                    new[] { $"{nameof(ImportBookingViewModel.Contacts)}[{index}].{nameof(ImportBookingOrderViewModel.ProductCode)}" });

                                importingResult.Add(error);
                            }
                        }
                        else
                        {
                            var error = new System.ComponentModel.DataAnnotations.ValidationResult($"{nameof(ImportBookingOrderViewModel.CustomerPONumber)} is not existing.",
                                new[] { $"{nameof(ImportBookingViewModel.Contacts)}[{index}].{nameof(ImportBookingOrderViewModel.CustomerPONumber)}" });

                            importingResult.Add(error);
                        }
                    }
                    #endregion CustomerPOs

                    #region POFulfillmentItineraries
                    poFulfillment.Itineraries = new List<POFulfillmentItineraryModel>();
                    foreach (var leg in importVM.Legs)
                    {
                        var loadingPort = await _csfeApiClient.GetLocationByCodeAsync(leg.LoadingPort);
                        var dischargePort = await _csfeApiClient.GetLocationByCodeAsync(leg.DischargePort);
                        var carrier = string.IsNullOrWhiteSpace(leg.CarrierCode) ? null :
                            await _csfeApiClient.GetCarrierByCodeAsync(leg.CarrierCode);

                        var itinerary = new POFulfillmentItineraryModel
                        {
                            Sequence = importVM.Legs.ToList().IndexOf(leg) + 1,
                            CreatedDate = poFulfillment.CreatedDate,
                            CreatedBy = poFulfillment.CreatedBy,
                            UpdatedBy = poFulfillment.UpdatedBy,
                            UpdatedDate = poFulfillment.UpdatedDate,
                            ETADate = leg.ETADate,
                            ETDDate = leg.ETDDate,
                            ModeOfTransport = leg.ModeOfTransport,
                            CarrierId = carrier?.Id,
                            CarrierName = carrier?.Name,
                            LoadingPortId = loadingPort?.Id ?? 0,
                            LoadingPort = loadingPort?.LocationDescription ?? leg.LoadingPort,
                            DischargePortId = dischargePort?.Id ?? 0,
                            DischargePort = dischargePort?.LocationDescription ?? leg.DischargePort,
                            VesselFlight = leg.VesselFlight,
                            Status = POFulfillmentItinerayStatus.Active
                        };

                        poFulfillment.Itineraries.Add(itinerary);
                    }
                    #endregion POFulfillmentItineraries

                    if (importingResult.Count() > 0)
                    {
                        UnitOfWork.RollbackTransaction();
                        return new ImportBookingResult
                        {
                            Success = false,
                            Result = importingResult
                        };
                    }

                    // Customer prefix must be available
                    var customerOrg = await _csfeApiClient.GetOrganizationByIdAsync(principalContact.OrganizationId);
                    if (customerOrg == null || string.IsNullOrEmpty(customerOrg.CustomerPrefix))
                    {
                        var error = new System.ComponentModel.DataAnnotations.ValidationResult($"Customer prefix is not valid to create booking!",
                                new[] { $"Booking" });
                        importingResult.Add(error);

                        UnitOfWork.RollbackTransaction();
                        return new ImportBookingResult
                        {
                            Success = false,
                            Result = importingResult
                        };
                    }
                    poFulfillment.Number = await GenerateBookingNumber(customerOrg.CustomerPrefix, customerOrg.Id, poFulfillment.CreatedDate);
                    poFulfillment.Status = POFulfillmentStatus.Active;
                    poFulfillment.Stage = POFulfillmentStage.ForwarderBookingConfirmed;
                    poFulfillment.IsForwarderBookingItineraryReady = false;
                    poFulfillment.IsFulfilledFromPO = true;
                    poFulfillment.FulfillmentType = FulfillmentType.PO;
                    poFulfillment.FulfilledFromPOType = POType.Bulk;
                    poFulfillment.AgentAssignmentMode = "Change";

                    foreach (var load in poFulfillment.Loads)
                    {
                        load.LoadReferenceNumber = await GenerateBookingLoadNumber(DateTime.UtcNow);
                    }

                    await Repository.AddAsync(poFulfillment);
                    await UnitOfWork.SaveChangesAsync();

                    var event1061BC = new ActivityViewModel()
                    {
                        ActivityCode = Event.EVENT_1061,
                        POFulfillmentId = poFulfillment.Id,
                        ActivityDate = DateTime.UtcNow,
                        Remark = importVM.SONumber,
                        CreatedBy = poFulfillment.CreatedBy
                    };
                    await _activityService.TriggerAnEvent(event1061BC);

                    // Adjust subtract purchase order line items linking to purchase order fulfillment
                    AdjustQuantityOnPOLineItems(poFulfillment.Id, AdjustBalanceOnPOLineItemsType.Deduct);

                    // Create POFulfillmentBookingRequests

                    poFulfillment.BookingRequests = new List<POFulfillmentBookingRequestModel>();
                    var bookingRequest = await _ediSonBookingService.CreateBookingRequestAsync(userName, poFulfillment, false);

                    bookingRequest.SONumber = importVM.SONumber;
                    bookingRequest.BillOfLadingHeader = importVM.BillOfLadingHeader;

                    bookingRequest.CYClosingDate = importVM.CYClosingDate;
                    bookingRequest.CYEmptyPickupTerminalCode = importVM.CYEmptyPickupTerminalCode;
                    bookingRequest.CYEmptyPickupTerminalDescription = importVM.CYEmptyPickupTerminalDescription;

                    bookingRequest.CFSClosingDate = importVM.CFSClosingDate;
                    bookingRequest.CFSWarehouseCode = importVM.CFSWarehouseCode;
                    bookingRequest.CFSWarehouseDescription = importVM.CFSWarehouseDescription;

                    poFulfillment.BookingRequests.Add(bookingRequest);
                    await UnitOfWork.SaveChangesAsync();

                    UnitOfWork.CommitTransaction();

                    ProceedShippingOrderFormBackground(ShippingFormType.ShippingOrder, poFulfillment.Id, userName, FulfillmentType.PO);

                    return new ImportBookingResult
                    {
                        BookingId = poFulfillment.Id,
                        BookingRefNumber = bookingRequest.BookingReferenceNumber,
                        Success = true
                    };
                }

                // Update booking
                else
                {
                    var booking = await Repository.GetAsync(c => c.Id == bookingId, null,
                        c => c.Include(c => c.Contacts).Include(c => c.Orders).Include(c => c.Loads));

                    if (booking.Status == POFulfillmentStatus.Inactive || booking.Stage >= POFulfillmentStage.ShipmentDispatch)
                    {
                        var error = new System.ComponentModel.DataAnnotations.ValidationResult($"Cannot update booking because booking has been canceled or dispatched",
                               new[] { $"Booking {booking.Number}" });

                        importingResult.Add(error);

                        return new ImportBookingResult
                        {
                            Success = false,
                            Result = importingResult
                        };
                    }

                    UnitOfWork.BeginTransaction();
                    #region General
                    booking.Owner = importVM.Owner;
                    booking.ExpectedShipDate = importVM.ExpectedShipDate;
                    booking.ExpectedDeliveryDate = importVM.ExpectedDeliveryDate;
                    booking.Incoterm = importVM.Incoterm;
                    booking.ModeOfTransport = importVM.ModeOfTransport;
                    booking.LogisticsService = (LogisticsServiceType)importVM.LogisticsService;
                    booking.MovementType = (MovementType)importVM.MovementType;
                    booking.BookingDate = importVM.BookingDate;

                    var locations = (await _csfeApiClient.GetLocationsByCodesAsync(new List<string> {
                    importVM.ShipFrom,
                    importVM.ShipTo,
                    importVM.DeliveryPort,
                    importVM.ReceiptPort
                    })).ToList();

                    var shipFrom = locations.FirstOrDefault(x => x.Name == importVM.ShipFrom);
                    var shipTo = locations.FirstOrDefault(x => x.Name == importVM.ShipTo);
                    var deliveryPort = locations.FirstOrDefault(x => x.Name == importVM.DeliveryPort);
                    var receiptPort = locations.FirstOrDefault(x => x.Name == importVM.ReceiptPort);

                    if (shipFrom != null)
                    {
                        booking.ShipFrom = shipFrom.Id;
                        booking.ShipFromName = shipFrom.LocationDescription;
                    }

                    if (shipTo != null)
                    {
                        booking.ShipTo = shipTo.Id;
                        booking.ShipToName = shipTo.LocationDescription;
                    }

                    if (deliveryPort != null)
                    {
                        booking.DeliveryPortId = deliveryPort.Id;
                        booking.DeliveryPort = deliveryPort.LocationDescription;
                    }

                    if (receiptPort != null)
                    {
                        booking.ReceiptPortId = receiptPort.Id;
                        booking.ReceiptPort = receiptPort.LocationDescription;
                    }

                    if (importVM.IsPropertyDirty(nameof(importVM.CargoReadyDate)))
                    {
                        booking.CargoReadyDate = importVM.CargoReadyDate;
                    }

                    if (importVM.IsPropertyDirty(nameof(importVM.ExpectedDeliveryDate)))
                    {
                        booking.ExpectedDeliveryDate = importVM.ExpectedDeliveryDate;
                    }

                    if (importVM.IsPropertyDirty(nameof(importVM.PreferredCarrier)))
                    {
                        if (!string.IsNullOrWhiteSpace(importVM.PreferredCarrier))
                        {
                            var preferredCarrier = await _csfeApiClient.GetCarrierByCodeAsync(importVM.PreferredCarrier);
                            booking.PreferredCarrier = preferredCarrier.Id;
                        }
                        else
                        {
                            booking.PreferredCarrier = 0;
                        }
                    }

                    if (importVM.IsPropertyDirty(nameof(importVM.Remarks)))
                    {
                        booking.Remarks = importVM.Remarks;
                    }

                    if (importVM.IsPropertyDirty(nameof(importVM.IsContainDangerousGoods)))
                    {
                        booking.IsContainDangerousGoods = Convert.ToBoolean((int)importVM.IsContainDangerousGoods);
                    }

                    if (importVM.IsPropertyDirty(nameof(importVM.IsShipperPickup)))
                    {
                        booking.IsShipperPickup = Convert.ToBoolean((int)importVM.IsShipperPickup);
                    }

                    if (importVM.IsPropertyDirty(nameof(importVM.IsNotifyPartyAsConsignee)))
                    {
                        booking.IsNotifyPartyAsConsignee = Convert.ToBoolean((int)importVM.IsNotifyPartyAsConsignee);
                    }

                    if (importVM.IsPropertyDirty(nameof(importVM.IsBatteryOrChemical)))
                    {
                        booking.IsBatteryOrChemical = Convert.ToBoolean((int)importVM.IsBatteryOrChemical);
                    }

                    if (importVM.IsPropertyDirty(nameof(importVM.IsCIQOrFumigation)))
                    {
                        booking.IsCIQOrFumigation = Convert.ToBoolean((int)importVM.IsCIQOrFumigation);
                    }

                    if (importVM.IsPropertyDirty(nameof(importVM.IsExportLicence)))
                    {
                        booking.IsExportLicence = Convert.ToBoolean((int)importVM.IsExportLicence);
                    }

                    if (importVM.IsPropertyDirty(nameof(importVM.VesselName)))
                    {
                        booking.VesselName = importVM.VesselName;
                    }
                    if (importVM.IsPropertyDirty(nameof(importVM.VoyageNo)))
                    {
                        booking.VoyageNo = importVM.VoyageNo;
                    }
                    if (importVM.IsPropertyDirty(nameof(importVM.UpdatedBy)))
                    {
                        booking.UpdatedBy = importVM.UpdatedBy;
                    }
                    if (importVM.IsPropertyDirty(nameof(importVM.UpdatedDate)))
                    {
                        booking.UpdatedDate = importVM.UpdatedDate;
                    }
                    if (importVM.IsPropertyDirty(nameof(importVM.CreatedBy)))
                    {
                        booking.CreatedBy = importVM.CreatedBy;
                    }
                    if (importVM.IsPropertyDirty(nameof(importVM.CreatedDate)))
                    {
                        booking.CreatedDate = importVM.CreatedDate;
                    }
                    #endregion General

                    #region Contacts
                    booking.Contacts.Clear();
                    var contactOrgCodes = importVM.Contacts
                   .Select(x => x.OrganizationCode)
                   .Where(x => x != "0");
                    var organizations = await _csfeApiClient.GetOrganizationsByCodesAsync(contactOrgCodes);
                    foreach (var item in importVM.Contacts)
                    {
                        var poffContact = new POFulfillmentContactModel();
                        Mapper.Map(item, poffContact);

                        if (item.OrganizationCode != "0")
                        {
                            var organization = organizations.FirstOrDefault(x => x.Code == item.OrganizationCode);
                            if (organization != null)
                            {
                                poffContact.OrganizationId = organization.Id;
                                poffContact.CompanyName = organization.Name;
                                poffContact.Address = organization.Address;
                                poffContact.AddressLine2 = organization.AddressLine2;
                                poffContact.AddressLine3 = organization.AddressLine3;
                                poffContact.AddressLine4 = organization.AddressLine4;
                                poffContact.ContactName = organization.ContactName;
                                poffContact.ContactNumber = organization.ContactNumber;
                                poffContact.ContactEmail = organization.ContactEmail;
                            }
                        }
                        poffContact.CreatedBy = booking.CreatedBy;
                        poffContact.CreatedDate = booking.CreatedDate;
                        poffContact.UpdatedBy = booking.UpdatedBy;
                        poffContact.UpdatedDate = booking.UpdatedDate;

                        booking.Contacts.Add(poffContact);
                    }
                    #endregion Contact

                    #region Customer POs
                    booking.Orders.Clear();
                    AdjustQuantityOnPOLineItems(booking.Id, AdjustBalanceOnPOLineItemsType.Return);

                    var principalContact = booking.Contacts.First(x => x.OrganizationRole == OrganizationRole.Principal);

                    var poNumbers = importVM.CustomerPOs.Select(x => x.CustomerPONumber);
                    var pos = await _purchaseOrderRepository.Query(
                        x => poNumbers.Contains(x.PONumber)
                        && x.Contacts.Any(c => c.OrganizationRole == OrganizationRole.Principal && c.OrganizationId == principalContact.OrganizationId), null, i => i.Include(y => y.LineItems)).ToListAsync();

                    foreach (var (item, index) in importVM.CustomerPOs.Select((item, index) => (item, index)))
                    {
                        var po = pos.FirstOrDefault(x => x.PONumber == item.CustomerPONumber);
                        if (po != null)
                        {
                            var poffOrder = Mapper.Map<POFulfillmentOrderModel>(item);
                            poffOrder.PurchaseOrderId = po.Id;
                            poffOrder.Status = POFulfillmentOrderStatus.Active;

                            var lineItem = po.LineItems.FirstOrDefault(x => x.ProductCode == poffOrder.ProductCode);
                            if (lineItem != null)
                            {
                                poffOrder.POLineItemId = lineItem.Id;
                                poffOrder.ProductName = lineItem.ProductName;

                                // calculate balance qty.
                                poffOrder.OrderedUnitQty = lineItem.OrderedUnitQty;
                                poffOrder.BalanceUnitQty = lineItem.OrderedUnitQty - poffOrder.FulfillmentUnitQty;

                                poffOrder.CreatedBy = booking.CreatedBy;
                                poffOrder.CreatedDate = booking.CreatedDate;
                                poffOrder.UpdatedBy = booking.UpdatedBy;
                                poffOrder.UpdatedDate = booking.UpdatedDate;

                                booking.Orders.Add(poffOrder);

                                // If the current stage = Released, Update PO stage = Booking Confirmed.
                                if (po.Stage == POStageType.Released)
                                {
                                    po.Stage = POStageType.ForwarderBookingConfirmed;
                                }
                            }
                            else
                            {
                                var error = new System.ComponentModel.DataAnnotations.ValidationResult($"{nameof(ImportBookingOrderViewModel.ProductCode)} is not existing.",
                                    new[] { $"{nameof(ImportBookingViewModel.Contacts)}[{index}].{nameof(ImportBookingOrderViewModel.ProductCode)}" });

                                importingResult.Add(error);
                            }
                        }
                        else
                        {
                            var error = new System.ComponentModel.DataAnnotations.ValidationResult($"{nameof(ImportBookingOrderViewModel.CustomerPONumber)} is not existing.",
                                new[] { $"{nameof(ImportBookingViewModel.Contacts)}[{index}].{nameof(ImportBookingOrderViewModel.CustomerPONumber)}" });

                            importingResult.Add(error);
                        }
                    }
                    #endregion Customer POs

                    #region Loads
                    booking.Loads.Clear();
                    foreach (var item in importVM.Loads)
                    {
                        var poffLoad = Mapper.Map<POFulfillmentLoadModel>(item);
                        poffLoad.Status = POFulfillmentLoadStatus.Active;
                        poffLoad.CreatedBy = booking.CreatedBy;
                        poffLoad.CreatedDate = booking.CreatedDate;
                        poffLoad.UpdatedBy = booking.UpdatedBy;
                        poffLoad.UpdatedDate = booking.UpdatedDate;
                        poffLoad.LoadReferenceNumber = await GenerateBookingLoadNumber(DateTime.UtcNow);
                        booking.Loads.Add(poffLoad);
                    }
                    #endregion Loads

                    await UnitOfWork.SaveChangesAsync();
                    UnitOfWork.CommitTransaction();
                    AdjustQuantityOnPOLineItems(booking.Id, AdjustBalanceOnPOLineItemsType.Deduct);
                    ProceedShippingOrderFormBackground(ShippingFormType.ShippingOrder, booking.Id, userName, FulfillmentType.PO);
                    SendEmailWhenBookingIsAmendedBackground(booking, matchedPOFFBookingRequest.BookingReferenceNumber);
                    return new ImportBookingResult
                    {
                        BookingId = bookingId,
                        BookingRefNumber = matchedPOFFBookingRequest?.BookingReferenceNumber,
                        Success = true
                    };
                }
            }
        }

        private void SendEmailWhenBookingIsAmendedBackground(POFulfillmentModel booking, string referenceNumber)
        {
            var originAgent = booking.Contacts.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.OriginAgent, StringComparison.OrdinalIgnoreCase));
            if (originAgent != null)
            {
                //collect all equipment types of the booking
                var equipmentTypes = booking.Loads?.Select(x => EnumHelper<EquipmentType>.GetDisplayDescription(x.EquipmentType)).ToList();

                var shipper = booking.Contacts.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.Shipper, StringComparison.OrdinalIgnoreCase));
                var consignee = booking.Contacts.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.Consignee, StringComparison.OrdinalIgnoreCase));

                var bookingEmailModel = new POFulfillmentEmailViewModel()
                {
                    Name = originAgent.ContactName,
                    BookingRefNumber = referenceNumber,
                    Shipper = shipper?.CompanyName,
                    Consignee = consignee?.CompanyName,
                    ShipFrom = booking.ShipFromName,
                    ShipTo = booking.ShipToName,
                    CargoReadyDate = booking.CargoReadyDate,
                    EquipmentTypes = equipmentTypes,
                    DetailPage = $"{_appConfig.ClientUrl}/po-fulfillments/view/{booking.Id}",
                    SupportEmail = _appConfig.SupportEmail
                };
                _queuedBackgroundJobs.Enqueue<SendMailBackgroundJobs>(x => x.SendMailAsync($"Booking has been amended #{booking.Id}", "Booking_Amended", bookingEmailModel,
                    originAgent.ContactEmail, $"Shipment Portal: Booking is amended ({referenceNumber} - {booking.ShipFromName})"));
            }
        }


        public async Task WriteImportingValidationLogAsync(List<System.ComponentModel.DataAnnotations.ValidationResult> validationResult, bool success, ImportBookingViewModel importData, string profile)
        {
            var logModel = new IntegrationLogModel
            {
                APIName = "Import Freight Booking",
                APIMessage = $"Owner: {importData.Owner} \nCreatedDate: {importData.CreatedDate}",
                EDIMessageRef = string.Empty,
                EDIMessageType = string.Empty,
                PostingDate = DateTime.UtcNow,
                Profile = profile,
                Status = success ? IntegrationStatus.Succeed : IntegrationStatus.Failed,
                Remark = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} GMT",
                Response = JsonConvert.SerializeObject(validationResult, Formatting.Indented)
            };
            await _integrationLogRepository.AddAsync(logModel);
            await UnitOfWork.SaveChangesAsync();
        }

        public async Task WriteImportingResultLogAsync(ImportBookingResult importingResult, ImportBookingViewModel importData, string profile)
        {
            string response = "";
            if (importingResult.Success)
            {
                response = JsonConvert.SerializeObject(importingResult, Formatting.Indented);
            }
            else
            {
                response = JsonConvert.SerializeObject(importingResult.Result, Formatting.Indented);
            }
            var logModel = new IntegrationLogModel
            {
                APIName = "Import Freight Booking",
                APIMessage = $"Owner: {importData.Owner} \nCreatedDate: {importData.CreatedDate}",
                EDIMessageRef = string.Empty,
                EDIMessageType = string.Empty,
                PostingDate = DateTime.UtcNow,
                Profile = profile,
                Status = importingResult.Success ? IntegrationStatus.Succeed : IntegrationStatus.Failed,
                Remark = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} GMT",
                Response = response
            };
            await _integrationLogRepository.AddAsync(logModel);
            await UnitOfWork.SaveChangesAsync();
        }
    }
}
