using Groove.SP.Application.Common;
using Groove.SP.Application.MasterDialog.ViewModels;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.Application.MasterDialog.Services.Interfaces
{
    public interface IMasterDialogService : IServiceBase<MasterDialogModel, MasterDialogViewModel>
    {
        /// <summary>
        /// Return list of Master Dialog by searched parameters in kendoGrid
        /// </summary>
        /// <param name="request"></param>
        /// <param name="isInternal"></param>
        /// <param name="affiliates"></param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        Task<DataSourceResult> ListAsync(DataSourceRequest request, bool isInternal, string affiliates = "", long? organizationId = 0);

        /// <summary>
        /// Get Master dialog detail by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isInternal"></param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        Task<MasterDialogViewModel> GetAsync(long id, bool isInternal, long organizationId);

        /// <summary>
        /// To delete master dialog by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isInternal"></param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        Task<bool> DeleteByKeysAsync(long id, bool isInternal, long organizationId);

        /// <summary>
        /// Return a dropdown list of number by searching value depending on filter criteria 
        /// (Master BL No., House BL No., Container No., Purchase Order No., Booking No., Shipment No.)
        /// </summary>
        /// <param name="filterCriteria"></param>
        /// <param name="filterValue"></param>
        /// <param name="isInternal"></param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        Task<IEnumerable<DropDownListItem<string>>> SearchNumberByFilterCriteriaAsync(string filterCriteria, string filterValue, bool isInternal, long? organizationId = 0);

        Task<IEnumerable<MasterDialogPOListItemViewModel>> SearchListOfPurchaseOrdersAsync(string messageShownOn, string filterCriteria, string filterValue, string searchTerm,
            int skip, int take, string returnType = POListReturnType.CHILD_LEVEL);

        Task<IEnumerable<MasterDialogPOListItemViewModel>> SearchListOfPurchaseOrdersByMasterDialogIdAsync(long masterDialogId, string searchTerm, int skip, int take);

        Task<MasterDialogViewModel> CreateAsync(MasterDialogViewModel viewModel, string userName);
        Task<MasterDialogViewModel> UpdateAsync(long id, MasterDialogViewModel viewModel, string userName, bool isInternal, long organizationId);
    }
}