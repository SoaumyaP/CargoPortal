using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Groove.SP.Application.BillOfLading.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.Shipments.ViewModels;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.BillOfLading.Services.Interfaces
{
    public interface IBillOfLadingService : IServiceBase<BillOfLadingModel, BillOfLadingViewModel>
    {
        Task<BillOfLadingViewModel> GetBOLAsync(string billOfLadingNoOrId, bool isInternal, string affiliates = "");

        Task<IEnumerable<BillOfLadingViewModel>> GetBOLsByMasterBOLAsync(long masterBillOfLadingId, bool isInternal, string affiliates = "");

        Task<QuickTrackBillOfLadingViewModel> GetQuickTrackAsync(string billOfLadingNo);

        Task<DataSourceResult> ListAsync(DataSourceRequest request, bool isInternal, long roleId, long organizationId, string affiliates = "");

        /// <summary>
        /// This method is used when user search HouseBL No as user is assigning House BL to Shipment via GUI
        /// </summary>
        /// <param name="houseBLNo"></param>
        /// <param name="modeOfTransport"></param>
        /// <param name="executionAgent"></param>
        /// <param name="isInternal"></param>
        /// <param name="affiliates"></param>
        /// <returns></returns>
        Task<IEnumerable<HouseBLQueryModel>> SearchHouseBLAsync(string houseBLNo, string modeOfTransport, long? executionAgent, bool isInternal, string affiliates);

        /// <summary>
        /// To assign master bill of lading to house bill of lading, via GUI (table MasterBills - BillOfLadings)
        /// </summary>
        /// <param name="houseBOLId">Bill of lading id (BillOfLadings table)</param>
        /// <param name="masterBOLId">Master bill id (MasterBills table)</param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task<bool> AssignMasterBLToHouseBLAsync(long houseBOLId, long masterBOLId, string userName);

        /// <summary>
        ///  To get data source for auto-complete as user is searching Shipment to link to House BL
        /// </summary>
        /// <param name="houseBLId">Bill of lading id (BillOfLadings table)</param>
        /// <param name="shipmentNo">shipmentNo is typing</param>
        /// <param name="modeOfTransport">modeOfTransport (BillOfLadings table)</param>
        /// <param name="executionAgentId">executionAgentId (BillOfLadings table)</param>
        /// <returns></returns>
        Task<IEnumerable<ShipmentViewModel>> SearchShipmentAsync(long houseBLId, string shipmentNo, string modeOfTransport, long executionAgentId, bool isInternal, string affiliates);

        /// <summary>
        /// To get list of house bill of lading which matches with filter set as user adding House BL from Master BL on GUI
        /// </summary>
        /// <param name="searchTerm">Text to search</param>
        /// <param name="isInternal">Is internal user</param>
        /// <param name="affiliates">Affiliate array string if external user</param>
        /// <returns>List of master bill of ladings sorted by master bl number asc</returns>
        Task<IEnumerable<BillOfLadingViewModel>> GetBillOfLadingListBySearchingNumberAsync(string searchTerm, bool isInternal, string affiliates);

        /// To unlink shipment
        /// </summary>
        /// <param name="houseBLId">BillOfLadingId</param>
        /// <param name="shipmentId">ShipmentId need unlink</param>
        /// <param name="isTheLastLinkedShipment">If the removed shipment is the last one in the list: Remove records in BillOfLadingItineraries - BillOfLadingContacts table</param>
        /// <param name="userName">Name of current user login</param>
        /// <returns></returns>
        void UnlinkShipment(long houseBLId, long shipmentId, int isTheLastLinkedShipment, string userName);

        /// <summary>
        /// To check HouseBL has already exists before add new
        /// </summary>
        /// <param name="houseBLNo">BillOfLadingNo</param>
        /// <returns>Return True if HouseBL has already exists</returns>
        Task<bool> CheckHouseBLAlreadyExistsAsync(string houseBLNo);
    }
}
