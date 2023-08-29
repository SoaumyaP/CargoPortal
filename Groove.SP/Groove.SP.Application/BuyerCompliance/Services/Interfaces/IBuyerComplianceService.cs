using System.Collections.Generic;
using System.Threading.Tasks;
using Groove.SP.Application.BuyerCompliance.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;


namespace Groove.SP.Application.BuyerComplianceService.Services.Interfaces
{
    public interface IBuyerComplianceService : IServiceBase<BuyerComplianceModel, BuyerComplianceViewModel>
    {
        Task<IEnumerable<BuyerComplianceModel>> GetListByOrgIdsAsync(List<long> orgIds);
        Task<BuyerComplianceModel> GetByOrgIdAsync(long organizationId);
        Task<BuyerComplianceModel> GetByPOFFIdAsync(long poffId);
        Task<IEnumerable<DropDownListItem<long>>> GetDropDownByProgressCheckStatusAsync(int roleId,string affiliates, bool isProgressCargoReadyDate = true);
        Task<IEnumerable<DropDownListItem<long>>> GetWarehouseServiceTypeDropDownAsync();

        Task<BuyerComplianceModel> GetByPOFFAsync(POFulfillmentModel poff);
        Task<IEnumerable<BuyerComplianceViewModel>> GetListAsync(long? organizationId);
        Task<BuyerComplianceViewModel> GetAsync(long id);
        Task<BuyerComplianceViewModel> CreateAsync(SaveBuyerComplianceViewModel model);
        Task<BuyerComplianceViewModel> UpdateAsync(SaveBuyerComplianceViewModel model);

        Task<BuyerComplianceViewModel> ActivateAsync(ActivateBuyerComplianceViewModel model);

        Task<bool> IsOrganizationExists(long id, long organizationId);
        /// <summary>
        /// To get Principal organizations that link to provided Agent organization 
        /// </summary>
        /// <param name="organizationId">Agent organization id</param>
        /// <returns></returns>
        Task<IEnumerable<DropDownListItem>> GetPrincipalSelectionsForAgentRoleAsync(long organizationId);
    }
}
