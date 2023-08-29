using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Groove.SP.Application.Common;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using Groove.SP.Application.Shipments.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.POFulfillment.Services.Interfaces
{
    public interface IPOFulfillmentService : IServiceBase<POFulfillmentModel, POFulfillmentViewModel>
    {
        /// <summary>
        /// Send mail to ask Principal for the missing POs (in dbo.POFulfillmentOrders) of the booking.
        /// </summary>
        /// <param name="bookingId"></param>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        Task AskMissingPOAsync(long bookingId, InputPOFulfillmentViewModel viewModel);

        Task<POFulfillmentViewModel> GetAsync(long id, bool isInternal, string affiliates);

        Task<POFulfillmentViewModel> CreateAsync(InputPOFulfillmentViewModel model, IdentityInfo currentUser);

        Task<POFulfillmentViewModel> UpdateAsync(InputPOFulfillmentViewModel model, IdentityInfo currentUser);

        Task<POFulfillmentViewModel> UpdateAsync(EdiSonUpdateConfirmPOFFViewModel model, IdentityInfo currentUser);

        Task<POFulfillmentViewModel> ValidateBookingAsync(POFulfillmentModel poff, string userName, string companyName);

        /// <summary>
        /// To inform booking validation result, not take any action on data updates
        /// </summary>
        /// <param name="poffId">Purchase order fulfillment id</param>
        /// <param name="currentUser">Current user</param>
        /// <returns></returns>
        Task<BookingValidationResult> TrialValidateBookingAsync(long poffId, IdentityInfo currentUser);

        /// <summary>
        /// To check data constraint on purchase order fulfillment before creating booking
        /// Call as: 1. Creating booking 2. Trail validating booking (policy).
        /// </summary>
        /// <param name="poff">Purchase order fulfillment</param>
        /// <param name="currentUser">Current user</param>
        /// <returns></returns>
        Task CheckPOFFDataBeforeCreatingBookingAsync(POFulfillmentModel poff, IdentityInfo currentUser);

        Task<POFulfillmentViewModel> CancelPOFulfillmentAsync(long id, string userName, CancelPOFulfillmentViewModel cancelModel);

        Task<POFulfillmentViewModel> CreateBookingAsync(long id, IdentityInfo currentUser);

        /// <summary>
        /// To amend purchase order fulfillment.
        /// </summary>
        /// <param name="poffId">Purchase order fulfillment id</param>
        /// <param name="userName">Current user name</param>
        /// <returns></returns>
        Task AmendPOFulfillmentAsync(long poffId, string userName);

        Task PlanToShipAsync(long id, string userName);

        /// <summary>
        /// To update POFulfillment Load Plan (Re-calculate POFulfillmentOrders, Update POFulfillmentLoads & POFulfillmentLoadDetails)
        /// and Plan to ship again.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task ReloadAsync(long id, InputPOFulfillmentViewModel model, IdentityInfo currentUser);

        Task DispatchAsync(long id, string userName);

        Task UpdateActivityAsync(List<long> poffIds, string eventCode, DateTime activityDate, string location, string remark = null);

        /// <summary>
        /// To update booking(s) and associated purchase orders to Shipment Dispatch.
        /// </summary>
        /// <param name="poffIds"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task ChangeStageToShipmentDispatchAsync(List<long> poffIds, string userName);

        /// <summary>
        /// To update booking(s) and associated purchase orders to Closed.
        /// <br></br>
        /// NOTES: Also trigger event 1071-Booking Closed and 1010-PO Closed
        /// </summary>
        /// <param name="poffIds"></param>
        /// <param name="userName"></param>
        /// <param name="eventDate"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        Task ChangeStageToClosedAsync(List<long> poffIds, string userName, DateTime eventDate, string location = null, string remark = null);

        /// <summary>
        /// To revert booking(s) and associated purchase orders to FB Confirm.
        /// </summary>
        /// <param name="poffIds"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task RevertStageToFBConfirmedAsync(List<long> poffIds, string userName);

        /// <summary>
        /// To revert booking(s) and associated purchase orders to FB Confirm.
        /// <br></br>
        /// NOTES: Also delete event 1071-Booking Closed and 1010-PO Closed
        /// </summary>
        /// <param name="poffIds"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task RevertStageToShipmentDispatchAsync(List<long> poffIds, string userName);

        Task ConfirmItinerariesFromShipmentAsync(long shipmentId, ShipmentConfirmItineraryViewModel model, string userName, string companyName);

        Task UpdateConfirmItineraryFromShipmentAsync(long shipmentId, ShipmentConfirmItineraryViewModel model, string userName);

        /// <summary>
        /// To get only the top priority purchase order adhoc change, 1 > 2 > 3 (smaller is on top)
        /// </summary>
        /// <param name="poffId">Purchase order fulfillment id</param>
        /// <returns></returns>
        Task<PurchaseOrderAdhocChangeViewModel> GetPurchaseOrderAdhocChangesTopPriorityAsync(long poffId);

        /// <summary>
        /// To all purchase order adhoc change, 1 > 2 > 3 (smaller is on top)
        /// </summary>
        /// <param name="poffId">Purchase order fulfillment id</param>
        /// <returns></returns>
        Task<IEnumerable<PurchaseOrderAdhocChangeViewModel>> GetPurchaseOrderAdhocChangesAsync(long poffId);

        /// <summary>
        /// To clean-up purchase order adhoc changes
        /// </summary>
        /// <param name="id">Record id</param>
        /// <param name="poffId">Purchase order fulfillment id</param>
        /// <param name="purchaseOrderId">Purchase order id</param>
        void DeletePurchaseOrderAdhocChanges(long id, long poffId, long purchaseOrderId);

        /// <summary>
        /// As POFF is in stage Forwarder Booking Request, then system will create ediSON/outport shipment for the purchase order fulfillment
        /// There are 2 ways to call it: 1. Booking validation is accepted / 2. Approve pending approval
        /// </summary>
        /// <param name="purchaseOrderFulfillmentId"></param>
        /// <param name="userName"></param>
        /// <param name="calledFrom">1. Booking validation is accepted / 2. Approve pending approval</param>
        /// <returns></returns>
        Task ProceedBookingForPurchaseOrderFulfillment(long purchaseOrderFulfillmentId, string userName, ActionCalledFrom calledFrom);

        /// <summary>
        /// As POFF is in stage Forwarder Booking Request and Itinerary confirmed, then system will move the purchase order fulfillment to Forwarder Booking Confirm
        /// There are 3 ways to call it: 1. Add/confirm itinerary on POFF UI / 2. Add/confirm itinerary on Shipment UI 3. Confirm via API ConfirmPOFFAsync (POFulfillmentItinerariesController.PostAsync)
        /// </summary>
        /// <param name="poffId"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task ConfirmPurchaseOrderFulfillmentAsync(long poffId, string shipmentNumber, string userName);

        Task DispatchAllocatedPurchaseOrderShipmentAsync(long shipmentId, string userName, DateTime eventDate, string location = null);

        Task ConfirmAllocatedPurchaseOrderShipmentAsync(long shipmentId, string userName);

        /// <summary>
        /// To adjust booked and balance quantity of purchase order line items
        /// .NOTES: Pls make sure to call it before deactivate all purchase order fulfillment loads
        /// .NOTES: It will also apply to all related allocated/ blanket purchase orders if any
        /// </summary>
        /// <param name="poffId">purchase order fulfillment id</param>
        /// <param name="typeEnum">Return to add / Deduct to subtract</param>
        void AdjustQuantityOnPOLineItems(long poffId, AdjustBalanceOnPOLineItemsType typeEnum);

        /// <summary>
        /// To deduct (-) booked and balance quantity of purchase order line items
        /// .NOTES: Pls make sure to call it before deactivate all purchase order fulfillment loads
        /// .NOTES: It will also apply to all related allocated/ blanket purchase orders if any
        /// </summary>
        /// <param name="poffId">purchase order fulfillment id</param>
        void DeductQuantityOnPOLineItems(long poffId);

        /// <summary>
        /// To release (+) booked and balance quantity of purchase order line items
        /// .NOTES: Pls make sure to call it before deactivate all purchase order fulfillment loads
        /// .NOTES: It will also apply to all related allocated/ blanket purchase orders if any
        /// </summary>
        /// <param name="poffId">purchase order fulfillment id</param>
        void ReleaseQuantityOnPOLineItems(long poffId);

        /// <summary>
        /// To update stage of purchase order as moving backward stage of purchase order fulfillment
        /// .NOTES: It will also update stage for all related allocated purchase orders if any
        /// </summary>
        /// <param name="poff"></param>
        /// <returns></returns>
        Task UpdatePurchaseOrderStageByPOFFAsync(POFulfillmentModel poff);

        /// <summary>
        /// To update stage of purchase order as moving backward stage of purchase order fulfillment
        /// .NOTES: It will also update stage for all related allocated purchase orders if any
        /// </summary>
        /// <param name="poff"></param>
        /// <returns></returns>
        Task UpdatePurchaseOrderStageByPOFFAsync(long poffId);

        Task<ImportBookingResult> ImportBookingAsync(ImportBookingViewModel importingModel, string userName);

        Task WriteImportingValidationLogAsync(List<System.ComponentModel.DataAnnotations.ValidationResult> importingResult, bool success, ImportBookingViewModel importData, string profile);

        Task WriteImportingResultLogAsync(ImportBookingResult importingResult, ImportBookingViewModel importData, string profile);
    }
}
