using Groove.SP.Application.Common;
using Groove.SP.Application.MasterBillOfLading.ViewModels;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.Application.MasterBillOfLading.Services.Interfaces
{
    public interface IMasterBillOfLadingService : IServiceBase<MasterBillOfLadingModel, MasterBillOfLadingViewModel>
    {
        Task<MasterBillOfLadingViewModel> GetAsync(long id, bool isInternal, string affiliates = "");
        Task<DataSourceResult> ListAsync(DataSourceRequest request, bool isInternal, long userRoleId, long organizationId, string affiliates = "");

        Task<MasterBillOfLadingViewModel> CreateAsync(CreateMasterBillOfLadingViewModel viewModel, string userName);
        /// <summary>
        /// To update master bill of lading via API called from EdiSON
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="userName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<MasterBillOfLadingViewModel> UpdateAsync(UpdateMasterBillOfLadingViewModel viewModel, string userName, long id);
        /// <summary>
        /// To update master bill of lading via GUI
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="userName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<MasterBillOfLadingViewModel> UpdateAsync(MasterBillOfLadingViewModel viewModel, string userName, long id);

        /// <summary>
        /// To get list of master bill of lading which matches with filter set as user adding Master BL from House BL on GUI
        /// </summary>
        /// <param name="searchTerm">Text to search</param>
        /// <param name="isDirectMaster">Filtering for direct master</param>
        /// <param name="isInternal">Is internal user</param>
        /// <param name="affiliates">Affiliate array string if external user</param>
        /// <returns>List of master bill of ladings sorted by master bl number asc</returns>
        Task<IEnumerable<MasterBillOfLadingViewModel>> GetMasterBillOfLadingListBySearchingNumberAsync(string searchTerm, bool isDirectMaster, bool isInternal, string affiliates);

        /// <summary>
        /// To assign house bill of lading to master bill of lading, via GUI (table MasterBills - BillOfLadings)
        /// </summary>
        /// <param name="masterBOLId">Master bill id (MasterBills table)</param>
        /// <param name="houseBOLId">Bill of lading id (BillOfLadings table)</param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task<bool> AssignHouseBLToMasterBLAsync(long masterBOLId, long houseBOLId, string userName);

        /// <summary>
        /// To remove house bill of lading from master bill of lading, via GUI (table MasterBills - BillOfLadings)
        /// </summary>
        /// <param name="masterBOLId">Master bill id (MasterBills table)</param>
        /// <param name="houseBOLId">Bill of lading id (BillOfLadings table)</param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task<bool> RemoveHouseBLFromMasterBLAsync(long masterBOLId, long houseBOLId, string userName);

        /// <summary>
        /// To create master bill of lading that refer to some data from house bill of lading, via GUI
        /// </summary>
        /// <param name="data">Master bill of lading data</param>
        /// <param name="houseBOLId">House bill of lading id</param>
        /// <param name="userName">User name/email</param>
        /// <returns></returns>
        Task<MasterBillOfLadingViewModel> CreateFromHouseBillOfLadingAsync(MasterBillOfLadingViewModel data, long houseBOLId, string userName);

        /// <summary>
        /// To create master bill of lading that refer to some data from shipment, via GUI
        /// </summary>
        /// <param name="data">Master bill of lading data</param>
        /// <param name="shipmentId">Shipment id</param>
        /// <param name="userName">User name/email</param>
        /// <returns></returns>
        Task<MasterBillOfLadingViewModel> CreateFromShipmentAsync(MasterBillOfLadingViewModel data, long shipmentId, string userName);

    }
}
