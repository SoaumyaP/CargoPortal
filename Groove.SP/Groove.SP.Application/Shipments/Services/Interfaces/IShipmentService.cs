using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Groove.SP.Application.BillOfLading.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.ShipmentLoadDetails.ViewModels;
using Groove.SP.Application.Shipments.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;

namespace Groove.SP.Application.Shipments.Services.Interfaces
{
    public interface IShipmentService : IServiceBase<ShipmentModel, ShipmentViewModel>
    {
        Task<Top5OceanVolumeViewModel> GetTop5OceanVolumeAsync(bool isOrigin, bool isInternal, string affiliates = "", string statisticFilter = "");

        Task<IEnumerable<ShipmentViewModel>> GetShipmentsByBOLAsync(long billOfLadingId, bool isInternal, string affiliates = "");

        /// <summary>
        /// To load data grid of shipments on master bill of lading details page
        /// </summary>
        /// <param name="masterBOLId"></param>
        /// <param name="isDirectMaster"></param>
        /// <param name="isInternal"></param>
        /// <param name="affiliates"></param>
        /// <returns></returns>
        Task<IEnumerable<ShipmentViewModel>> GetShipmentsByMasterBOLAsync(long masterBOLId, bool isDirectMaster, bool isInternal, string affiliates = "");

        Task<ShipmentViewModel> GetAsync(string shipmentNo, bool isInternal, string affiliates = "");

        Task<QuickTrackShipmentViewModel> GetQuickTrackAsync(string shipmentNo);

        Task<DashBoardSummaryGridViewModel<Top10ThisWeekViewModel>> GetTop10ThisWeekAsync(string organizationRole, bool isInternal, string affiliates = "", string statisticFilter ="");

        Task<DashBoardSummaryGridViewModel<Top10ThisWeekViewModel>> GetTop10CarrierThisWeekAsync(bool isInternal, string affiliates = "", string statisticFilter = "");

        Task<ReportingMetricShipmentViewModel> GetReportingWeeklyShipments(bool isInternal, string affiliates, string statisticFilter);

        Task<WeeklyReportingMetricOceanVolumeViewModel> GetReportingWeeklyOceanVolume(bool isInternal, string affiliates, string statisticFilter);

        Task<MonthlyReportingOceanVolumeByMovementViewModel> GetReportingMonthlyOceanVolume(string groupBy, bool isInternal, string affiliates, string statisticFilter);

        Task<ShipmentViewModel> CancelShipmentAsync(long id, string userName);

        /// <summary>
        /// Trial validate on cancel shipment. Throw an error if validate failed.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task TrialValidationOnCancelShipmentAsync(long id);

        /// <summary>
        /// Trial validate on assign House BL into shipment. Throw an error if validate failed.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task TrialValidateOnAssignHouseBLAsync(long id);

        Task<IEnumerable<ShipmentExceptionViewModel>> GetExceptionsAsync(string idList);

        /// <summary>
        /// As the mother booking has multiple Child Shipments, This function determines if a shipment is the first to be dispatched
        /// </summary>
        /// <param name="shipmentId">Id of the Shipment</param>
        /// <returns></returns>
        Task<bool> IsFirstChildShipmentDispatch(long shipmentId);

        Task<bool> IsOtherChildShipmentsClosed(long poFulfillmentId, long shipmentId);

        Task RevertAllocatedPurchaseOrderToFBConfirmedAsync(long shipmentId, string userName);

        /// <summary>
        /// To get select options on shipments which are filtered by shipment number and cruise order (principal organization id)
        /// </summary>
        /// <param name="shipmentNumber">Shipment number</param>
        /// <param name="cruiseOrderId">Id of cruise order</param>
        /// <returns></returns>
        Task<IEnumerable<DropDownListItem<long>>> SearchShipmentsByShipmentNumberAsync(string shipmentNumber, long cruiseOrderId);

        /// <summary>
        /// To get select dropdown options on shipments are filtered by shipment number and Consolidation ID.
        /// <para>Return the list of Shipment No that has:</para>
        /// <para>+NO link with viewing Consolidation yet</para>
        /// <para>+Must have Cargo Details and matching with 1st associated Consignment/Shipment</para>
        /// <para>+Matching with the 1st associated Consignment/Shipment</para>
        /// </summary>
        /// <param name="consolidationId"></param>
        /// <param name="shipmentNumber"></param>
        /// <param name="isInternal"></param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        Task<IEnumerable<DropDownListItem<long>>> SearchShipmentNumberByConsolidationAsync(long consolidationId, string shipmentNumber, bool isInternal, long organizationId);

        /// <summary>
        /// To create linking to shipment and update some table
        /// </summary>
        /// <param name="shipmentId"></param>
        /// <param name="billOfLadingId"></param>
        /// <param name="executionAgentId"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task AssignHouseBLToShipmentAsync(long shipmentId, long billOfLadingId, long executionAgentId, string userName);

        /// <summary>
        /// To create new HouseBL and linking to shipment
        /// </summary>
        /// <param name="shipmentId">ShipmenId need link</param>
        /// <param name="BillOfLadingViewModel">View model</param>
        /// <returns></returns>
        Task CreateAndAssignHouseBLAsync(long shipmentId, BillOfLadingViewModel billOfLadingViewModel, string userName);

        /// <summary>
        /// Return true if all shipment's cargo items (dbo.CargoDetails) already loaded into containers (dbo.ShipmentLoadDetails).
        /// </summary>
        /// <param name="shipmentId"></param>
        /// <returns></returns>
        Task<bool> IsFullLoadShipmentAsync(long shipmentId);

        /// <summary>
        /// To assign Master Bill of Lading to Shipment
        /// </summary>
        /// <param name="shipmentId">Bill of lading id (BillOfLadings table)</param>
        /// <param name="masterBOLId">Master bill id (MasterBills table)</param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task<bool> AssignMasterBLToShipmentAsync(long shipmentId, long masterBOLId, string userName);

        /// <summary>
        /// To unlink master bill of lading from provided shipment, via GUI
        /// </summary>
        /// <param name="shipmentId">Bill of lading id (BillOfLadings table)</param>
        /// <param name="masterBOLId">Master bill id (MasterBills table)</param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task<bool> UnlinkMasterBillOfLadingAsync(long shipmentId, string userName);

        /// <summary>
        /// To edit shipment via GUI
        /// </summary>
        /// <param name="shipmentId">Shipment Id</param>
        /// <param name="viewModel">View model to update</param>
        /// <returns></returns>
        Task<bool> EditShipmentAsync(long shipmentId, UpdateShipmentViewModel viewModel, string userName);

        /// <summary>
        /// To get list of shipment load details of current shipment
        /// </summary>
        /// <param name="shipmentId"></param>
        /// <returns></returns>
        Task<IEnumerable<ShipmentLoadDetailViewModel>> GetShipmentLoadDetailsAsync(long shipmentId);

        Task<ImportingShipmentResultViewModel> ImportFreightShipmentAsync(ImportShipmentViewModel model, string userName);

        Task WriteImportingResultLogAsync(ImportingShipmentResultViewModel importingResult, ImportShipmentViewModel importData, string profile, string userName);

        Task<int> CountNumberOfEvent2054Async(long shipmentId);

        /// <summary>
        /// To get default CFS Closing Date if the 1st leg of shipment has the same vessel as another that confirmed itinerary and inputted the CFS Closing Date.
        /// </summary>
        /// <param name="shipmentId"></param>
        /// <returns></returns>
        Task<DateTime?> GetDefaultCFSClosingDateAsync(long shipmentId);

        Task<ShipmentMilestoneViewModel> GetCurrentMilestoneAsync(long shipmentId);
    }
}