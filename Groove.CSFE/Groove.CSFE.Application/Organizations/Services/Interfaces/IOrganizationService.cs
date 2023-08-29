using GGroove.CSFE.Application;
using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Organizations.ViewModels;
using Groove.CSFE.Application.WarehouseAssignments.ViewModels;
using Groove.CSFE.Application.WarehouseLocations.ViewModels;
using Groove.CSFE.Core;
using Groove.CSFE.Core.Entities;
using Groove.CSFE.Core.Models;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.Organizations.Services.Interfaces
{
    public interface IOrganizationService : IServiceBase<OrganizationModel, OrganizationViewModel>
    {
        Task<IEnumerable<OrganizationReferenceDataViewModel>> GetOrgReferenceDataSourceAsync(IEnumerable<long> ids);

        Task<OrganizationReferenceDataViewModel> GetOrgReferenceDataSourceAsync(string code);

        Task<IEnumerable<OrganizationViewModel>> GetByCodesAsync(IEnumerable<string> codes);

        Task<IEnumerable<OrganizationViewModel>> GetByIdsAsync(IEnumerable<long> ids);

        Task<IEnumerable<OrganizationViewModel>> GetByEdisonCompanyCodeIdsAsync(IEnumerable<string> edisonCompanyCodes);

        Task<OrganizationViewModel> GetAsync(long id);

        Task<OrganizationViewModel> GetAsync(string code);

        Task<OrganizationViewModel> GetByNameAsync(string name);
        Task<IEnumerable<OrganizationViewModel>> GetByFulltextSearchNameAsync(string name);

        Task<IEnumerable<OrganizationReferenceDataViewModel>> GetActiveOrgReferenceDataSourceAsync();

        Task<bool> HasCustomerPrefix(long id);

        Task<IEnumerable<OrganizationCodeViewModel>> GetActiveCodesListAsync();

        Task<IEnumerable<OrganizationCodeViewModel>> GetActiveCodesListExcludeIdsAsync(IEnumerable<long> excludeOrgIds);

        Task<bool> CheckCustomerPrefixNotTaken(long id, string customerPrefix);

        Task<bool> CheckContactEmailExists(long id);

        Task<IEnumerable<OrganizationViewModel>> GetAffiliatesAsync(long organizationId);
        Task<IEnumerable<OrganizationViewModel>> GetAffiliatesAsync(string organizationIds);

        Task<IEnumerable<DropDownModel>> OtherCodeDropDownAsync(long organizationId);

        Task<IEnumerable<DropDownModel>> GetAffiliateDropdownAsync(long organizationId);

        Task<OrganizationViewModel> AddAffiliateAsync(long organizationId, long affiliateId, string userName);

        Task<IEnumerable<long>> GetAffiliateCodesAsync(long organizationId);


        Task<OrganizationViewModel> RemoveAffiliateAsync(long affiliateId, string userName);

        Task<OrganizationViewModel> UpdateAsync(long id, OrganizationViewModel viewModel, string username);

        Task<OrganizationViewModel> UpdateBuyerAsync(OrganizationViewModel viewModel, string username);

        Task<OrganizationViewModel> UpdateAdminUserAsync(OrganizationViewModel viewModel, string username);

        Task AssignWarehouseLocationAsync(long customerOrgId, WarehouseAssignmentViewModel model, string userName);

        Task DeleteWarehouseLocationAsync(long customerOrgId, long warehouseLocationId);

        #region Customers
        Task<IEnumerable<OrganizationViewModel>> GetCustomersAsync(long organizationId);
        Task AddCustomerAsync(long organizationId, long customerId, ConnectionType connectionType, string userName);
        Task RemoveCustomerAsync(long organizationId, long customerId, string userName);
        Task<IEnumerable<OrganizationViewModel>> GetBuyersAsync();
        Task<IEnumerable<OrganizationViewModel>> GetBuyersAsync(long organizationId);
        Task ResendConnectionToCustomer(long supplierId, long customerId, string userName);
        Task ResendConnectionToSupplier(long supplierId, long customerId, string userName);
        #endregion

        #region Suppliers
        Task<OrganizationViewModel> AddSupplierAsync(long customerId, SupplierViewModel supplierViewModel, string userName, StringValues authHeader);
        Task UpdateSupplierAsync(long customerId, long supplierId, SupplierViewModel supplierViewModel, string userName, StringValues authHeader);
        Task RemoveSupplierAsync(long customerId, long supplierId, string userName);
        Task<List<OrganizationCodeViewModel>> UpdateSupplierStatusAsync(long supplierId, string username);
        Task ConfirmConnectionTypeAsync(long supplierId, long customerId, bool isConfirm, string username);
        Task<IEnumerable<SupplierCustomerRelationshipViewModel>> GetCustomerRelationShips(IEnumerable<long> affiliateCodes, ConnectionType connectionType);
        #endregion

        /// <summary>
        /// To get Principal organizations depending on role
        /// </summary>
        /// <param name="isInternal"></param>
        /// <param name="roleId"></param>
        /// <param name="organizationId"></param>
        /// <param name="affiliates"></param>
        /// <returns></returns>
        Task<IEnumerable<DropDownListItem>> GetPrincipalSelectionsAsync(bool isInternal, long roleId, long organizationId, string affiliates, bool checkIsBuyer = true);

        /// <summary>
        /// To get Supplier organizations depending on selected principal
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        Task<IEnumerable<DropDownListItem>> GetSupplierSelectionsByPrincipalAsync(long customerId);

        Task<IEnumerable<long>> GetAccessibleOrganizationIdsForReportAsync(long roleId, long organizationId, string affiliates);

        Task<bool> ValidateReportPrincipal(long customerId, Role roleId, long requestingOrganizationId);

        Task<IEnumerable<DropDownListItem<long>>> GetAgentOrganizationDropDownListAsync();

        Task<IEnumerable<DropDownModel<long>>> GetSelectionsAsync(OrganizationType? type = null);
        
        /// <summary>
        /// To check if an email domain is existing on Agent organizations
        /// </summary>
        /// <param name="emailDomain"></param>
        /// <returns></returns>
        Task<bool> CheckAgentDomainAsync(string emailDomain);

        /// <summary>
        /// To search organizations by name to switch user role via GUI
        /// </summary>
        /// <param name="organizationName"></param>
        /// <returns></returns>
        Task<IEnumerable<SwitchOrganizationViewModel>> GetSwitchOrganizationSelectionsAsync(string organizationName);

        Task<bool> CheckParentInCustomerRelationshipAsync(long organizationId);

        /// <summary>
        /// To bulk insert organizations to database. Method will store data as same as inputted without data checking/validating/auditing.
        /// </summary>
        /// <param name="model">Data to insert</param>
        /// <returns>A number of record that were inserted</returns>
        Task<int> BulkInsertOrganizationsAsync(IEnumerable<BulkInsertOrganizationViewModel> model);
    }
}
