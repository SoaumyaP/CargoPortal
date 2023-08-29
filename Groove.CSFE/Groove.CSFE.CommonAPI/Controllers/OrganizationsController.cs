using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Groove.CSFE.Application.Authorization;
using Groove.CSFE.Application.Organizations.Services.Interfaces;
using Groove.CSFE.Application.Organizations.ViewModels;
using Groove.CSFE.Application.WarehouseAssignments.Services.Interfaces;
using Groove.CSFE.Application.WarehouseAssignments.ViewModels;
using Groove.CSFE.Application.WarehouseLocations.Services.Interfaces;
using Groove.CSFE.CommonAPI.Filters;
using Groove.CSFE.Core;
using Groove.CSFE.Core.Data;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Groove.CSFE.CommonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize]
    public class OrganizationsController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;
        private readonly AppConfig _appConfig;
        private readonly IOrganizationListService _organizationListService;
        private readonly IWarehouseLocationService _warehouseLocationService;
        private readonly IWarehouseAssignmentService _warehouseAssignmentService;

        public OrganizationsController(IOrganizationService organizationService, IOptions<AppConfig> appConfig,
            IWarehouseLocationService warehouseLocationService,
            IOrganizationListService organizationListService,
            IWarehouseAssignmentService warehouseAssignmentService)
        {
            _organizationService = organizationService;
            _warehouseLocationService = warehouseLocationService;
            _appConfig = appConfig.Value;
            _organizationListService = organizationListService;
            _warehouseAssignmentService = warehouseAssignmentService;
        }

        /// <summary>
        /// To get compact information on organizations as reference data.
        /// Please refer to API 'OrgReferenceData' to get set of data, not all data stored.
        /// </summary>
        /// <param name="idList">String of array of organization ids. Such as: 123,124,125</param>
        /// <returns></returns>
        [HttpGet]
        [Route("OrgReferenceData")]
        public async Task<IActionResult> GetOrgReferenceDataSourceAsync([FromQuery] string idList)
        {
            var ids = idList?.Split(',').Select(x => Convert.ToInt64(x));
            var viewModels = await _organizationService.GetOrgReferenceDataSourceAsync(ids);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("{code}/OrgReferenceData")]
        public async Task<IActionResult> GetOrgReferenceDataSourceByCodesAsync(string code)
        {
            var viewModel = await _organizationService.GetOrgReferenceDataSourceAsync(code);
            return new JsonResult(viewModel);
        }

        [HttpGet]
        [Route("AgentOrganization")]
        public async Task<IActionResult> GetAgentOrganizations()
        {
            var viewModels = await _organizationService.GetAsync(_appConfig.AgentOrganizationCode);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("{id}/WarehouseAssignments")]
        public async Task<IActionResult> GetWarehouseAssignmentsAsync(long id)
        {
            var viewModels = await _warehouseAssignmentService.GetByOrgIdAsync(id);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("{id}/warehouseProviders")]
        public async Task<IActionResult> GetWarehouseProvidersAsync(long id)
        {
            var viewModels = await _warehouseLocationService.GetByOrganizationAsync(id);

            return new JsonResult(viewModels?.Select(x => x.Organization));
        }

        [HttpPost]
        [Route("{id}/WarehouseAssignments")]
        public async Task<IActionResult> AssignWarehouseAsync(long id, [FromBody] WarehouseAssignmentViewModel model)
        {
            await _organizationService.AssignWarehouseLocationAsync(id, model, CurrentUser.Username);
            return Ok();
        }

        [HttpDelete]
        [Route("{id}/WarehouseAssignments/{warehouseLocationId}")]
        public async Task<IActionResult> DeleteWarehouseAsync(long id, long warehouseLocationId)
        {
            await _organizationService.DeleteWarehouseLocationAsync(id, warehouseLocationId);
            return Ok();
        }

        [HttpPost]
        [Route("Code")]
        public async Task<IActionResult> GetByCodes([FromBody] List<string> codes)
        {
            var viewModels = await (codes == null || !codes.Any()
                ? _organizationService.GetAllAsync()
                : _organizationService.GetByCodesAsync(codes));
            return Ok(viewModels);
        }

        [HttpGet]
        [Route("getByCode/{code}")]
        public async Task<IActionResult> GetByCode(string code)
        {
            var viewModels = await _organizationService.GetAsync(code);
            return Ok(viewModels);
        }

        [HttpGet]
        [Route("getByName/{name}")]
        public async Task<IActionResult> GetByNameAsync(string name)
        {
            var viewModel = await _organizationService.GetByNameAsync(name);
            return Ok(viewModel);
        }

        [HttpGet]
        [Route("getByFullTextSearchName/{name}")]
        public async Task<IActionResult> GetByFullTextSearchNameAsync(string name)
        {
            var viewModel = await _organizationService.GetByFulltextSearchNameAsync(name);
            return Ok(viewModel);
        }

        [HttpPost]
        [Route("OrganizationIds")]
        public async Task<IActionResult> GetByIdsAsync([FromBody] List<long> ids)
        {
            var viewModels = await (ids == null || !ids.Any()
                ? _organizationService.GetAllAsync()
                : _organizationService.GetByIdsAsync(ids));
            return Ok(viewModels);
        }

        [HttpPost]
        [Route("edisonCompanyCodeIds")]
        public async Task<IActionResult> GetByEdisonCompanyCodeIdsAsync([FromBody] List<string> codes)
        {
            var viewModels = await _organizationService.GetByEdisonCompanyCodeIdsAsync(codes);
            return Ok(viewModels);
        }

        /// <summary>
        /// To get compact information on active organizations as reference data.
        /// Please refer to API 'OrgReferenceData' to get set of data, not all data stored.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("OrgReferenceData/Active")]
        public async Task<IActionResult> GetActiveOrgReferenceDataSourceAsync()
        {
            var viewModels = await _organizationService.GetActiveOrgReferenceDataSourceAsync();
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("{id}/HasCustomerPrefix")]
        public async Task<IActionResult> HasCustomerPrefix(long id)
        {
            var viewModels = await _organizationService.HasCustomerPrefix(id);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("ActiveCodes")]
        public async Task<IActionResult> GetActiveCodesList()
        {
            var viewModels = await _organizationService.GetActiveCodesListAsync();
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("ActiveCodesExcludeIds")]
        public async Task<IActionResult> GetActiveCodesList([FromQuery] string excludeOrgIds)
        {
            var ids = excludeOrgIds?.Split(',').Select(x => Convert.ToInt64(x));
            if (ids == null || !ids.Any())
            {
                ids = new List<long>();
            }
            var viewModels = await _organizationService.GetActiveCodesListExcludeIdsAsync(ids);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("Search")]
        // temporarily disabled due to Org Admin can access
        //[AppAuthorize(AppPermissions.Organization_List)]
        [AppAuthorize]
        public async Task<IActionResult> Search([DataSourceRequest] DataSourceRequest request)
        {
            var viewModels = await _organizationListService.GetListOrganizationAsync(request);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("{id}/Affiliates")]
        public async Task<IActionResult> GetAffiliates(long id)
        {
            var viewModels = await _organizationService.GetAffiliatesAsync(id);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("AffiliatesByOrgIds")]
        public async Task<IActionResult> GetAffiliatesByOrgIds(string orgIds)
        {
            var viewModels = await _organizationService.GetAffiliatesAsync(orgIds);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("{id}/AffiliateCodes")]
        public async Task<IActionResult> GetAffiliateCodes(long id)
        {
            var viewModels = await _organizationService.GetAffiliateCodesAsync(id);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("CustomerRelationShips")]
        public async Task<IActionResult> CustomerRelationShips(string affiliates, int? connectionType = 1)
        {
            var affiliateCodes = JsonConvert.DeserializeObject<List<long>>(affiliates);
            Enum.TryParse<ConnectionType>(connectionType.ToString(), out var eConnectionType);
            var result = await _organizationService.GetCustomerRelationShips(affiliateCodes, eConnectionType);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{id}/OtherCodeDropDown")]
        public async Task<IActionResult> OtherCodeDropDown(long id)
        {
            var result = await _organizationService.OtherCodeDropDownAsync(id);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{id}/AffiliateDropdown")]
        public async Task<IActionResult> GetAffilidateDropdownByOrgIdAsync(long id)
        {
            var result = await _organizationService.GetAffiliateDropdownAsync(id);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{id}/OrganizationLogo")]
        public async Task<IActionResult> GetCompanyLogo(long id)
        {
            var viewModels = await _organizationService.GetAsync(id);
            return new JsonResult(new { id, viewModels.OrganizationLogo });
        }

        [HttpGet]
        [Route("{id}/CheckParentInCustomerRelationship")]
        public async Task<IActionResult> CheckParentInCustomerRelationshipAsync(long id)
        {
            var result = await _organizationService.CheckParentInCustomerRelationshipAsync(id);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{id}")]
        [AppAuthorize()]
        public async Task<IActionResult> GetById(long id)
        {
            var viewModels = await _organizationService.GetAsync(id);
            return new JsonResult(viewModels);
        }

        [HttpPut]
        [Route("{supplierId}/updateStatus")]
        [AppAuthorize()]
        public async Task<IActionResult> UpdateSupplierStatus(long supplierId)
        {
            var username = CurrentUser.Username;
            var customerIdList = await _organizationService.UpdateSupplierStatusAsync(supplierId, username);
            return new JsonResult(customerIdList);
        }

        [HttpPut]
        [Route("{supplierId}/confirmConnectionType/{customerId}")]
        [AppAuthorize()]
        public async Task<IActionResult> ConfirmConnectionType(long supplierId, long customerId, bool isConfirm)
        {
            var username = CurrentUser.Username;
            await _organizationService.ConfirmConnectionTypeAsync(supplierId, customerId, isConfirm, username);
            return Ok();
        }

        [HttpPost("CheckCustomerPrefix")]
        [AppAuthorize(AppPermissions.Organization_Detail_Edit)]
        public async Task<IActionResult> CheckCustomerPrefixNotTakenAsync([FromBody] CheckCustomerPrefixViewModel model)
        {
            var result = await _organizationService.CheckCustomerPrefixNotTaken(model.Id, model.CustomerPrefix);
            return new JsonResult(new { NotTaken = result });
        }

        [HttpGet("{id}/CheckContactEmail")]
        public async Task<IActionResult> CheckContactEmailAsync(long id)
        {
            var result = await _organizationService.CheckContactEmailExists(id);
            return new JsonResult(result);
        }

        [HttpPost]
        [Route("{id}/Affiliates")]
        public async Task<IActionResult> AddAffiliate(long id, [FromBody] OrganizationViewModel affiliate)
        {
            var username = CurrentUser.Username;
            var viewModels = await _organizationService.AddAffiliateAsync(id, affiliate.Id, username);
            return new JsonResult(viewModels);
        }

        [HttpDelete]
        [Route("{id}/Affiliates/{affiliateId}")]
        public async Task<IActionResult> RemoveAffiliate(long id, long affiliateId)
        {
            var username = CurrentUser.Username;
            var viewModels = await _organizationService.RemoveAffiliateAsync(affiliateId, username);
            return new JsonResult(viewModels);
        }

        [HttpPost]
        [AppAuthorize]

        public async Task<IActionResult> PostAsync([FromBody] OrganizationViewModel model)
        {
            var result = await _organizationService.CreateAsync(model);
            return Ok(result);
        }


        /// <summary>
        /// To bulk insert organization, data validation will be skipped
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("Bulk")]
        [AppAuthorize]
        public async Task<IActionResult> BulkInsertOrganizationsAsync(IEnumerable<BulkInsertOrganizationViewModel> model)
        {
            var result = await _organizationService.BulkInsertOrganizationsAsync(model);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [AppAuthorize]

        public async Task<IActionResult> Update(long id, [FromBody] OrganizationViewModel viewModel)
        {
            var username = CurrentUser.Username;
            var viewModels = await _organizationService.UpdateAsync(id, viewModel, username);
            return new JsonResult(viewModels);
        }

        [HttpDelete("{id}")]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]

        public async Task<IActionResult> DeleteAsync(long id)
        {
            var result = await _organizationService.DeleteAsync(id);
            return Ok(result);
        }

        [HttpPut("{id}/UpdateBuyer")]
        [AppAuthorize(AppPermissions.Organization_Compliance_Detail_Edit)]
        public async Task<IActionResult> UpdateBuyer(long id, [FromBody] OrganizationViewModel viewModel)
        {
            var username = CurrentUser.Username;
            var viewModels = await _organizationService.UpdateBuyerAsync(viewModel, username);
            return new JsonResult(viewModels);
        }

        [HttpPut("{id}/UpdateAdminUser")]
        public async Task<IActionResult> UpdateAdminUser(long id, [FromBody] OrganizationViewModel viewModel)
        {
            var username = CurrentUser.Username;
            var viewModels = await _organizationService.UpdateAdminUserAsync(viewModel, username);
            return new JsonResult(viewModels);
        }

        #region Customers
        [HttpGet]
        [Route("{id}/Customers")]
        public async Task<IActionResult> GetCustomers(long id)
        {
            var viewModels = await _organizationListService.GetListCustomerRelationshipAsync(id);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("Buyers")]
        [AppAuthorize(AppPermissions.PO_Fulfillment_Detail_Edit)]
        public async Task<IActionResult> GetBuyers()
        {
            var viewModels = await _organizationService.GetBuyersAsync();
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("{id}/Buyers")]
        // TODO: Shippers can get only their buyer list 
        [AppAuthorize(AppPermissions.PO_Fulfillment_Detail_Edit)]
        public async Task<IActionResult> GetBuyers(long id)
        {
            var viewModels = await _organizationService.GetBuyersAsync(id);
            return new JsonResult(viewModels);
        }

        [HttpPost]
        [Route("{id}/Customers")]
        public async Task<IActionResult> AddCustomer(long id, CustomerRelationshipViewModel viewModel)
        {
            var username = CurrentUser.Username;
            await _organizationService.AddCustomerAsync(id, viewModel.CustomerId, viewModel.ConnectionType, username);
            return Ok();
        }

        [HttpDelete]
        [Route("{id}/Customers/{customerId}")]
        public async Task<IActionResult> RemoveCustomer(long id, long customerId)
        {
            var username = CurrentUser.Username;
            await _organizationService.RemoveCustomerAsync(id, customerId, username);
            return Ok();
        }

        [HttpPut]
        [Route("{id}/ResendConnectionToCustomer/{customerId}")]
        public async Task<IActionResult> ResendConnectionToCustomer(long id, long customerId)
        {
            var username = CurrentUser.Username;
            await _organizationService.ResendConnectionToCustomer(id, customerId, username);
            return Ok();
        }

        [HttpPut]
        [Route("{id}/ResendConnectionToSupplier/{customerId}")]
        public async Task<IActionResult> ResendConnectionToSupplier(long id, long customerId)
        {
            var username = CurrentUser.Username;
            await _organizationService.ResendConnectionToSupplier(id, customerId, username);
            return Ok();
        }
        #endregion

        #region Suppliers
        [HttpGet]
        [AppAuthorize()]
        [Route("{customerId}/suppliers")]
        public async Task<IActionResult> GetSuppliers(long customerId)
        {
            var viewModels = await _organizationListService.GetSuppliersAsync(customerId);
            return new JsonResult(viewModels);
        }

        [HttpPost]
        [AppAuthorize()]
        [Route("{customerId}/suppliers")]
        public async Task<IActionResult> AddSupplier(long customerId, SupplierViewModel supplierViewModel)
        {
            var username = CurrentUser.Username;
            ControllerContext.HttpContext.Request.Headers.TryGetValue(HeaderNames.Authorization, out var authHeader);
            var result = await _organizationService.AddSupplierAsync(customerId, supplierViewModel, username, authHeader);
            return Ok(result);
        }

        [HttpPut]
        [AppAuthorize()]
        [Route("{customerId}/suppliers/{supplierId}")]
        public async Task<IActionResult> UpdateSupplierRelationship(long customerId, long supplierId, SupplierViewModel supplierViewModel)
        {
            var username = CurrentUser.Username;
            ControllerContext.HttpContext.Request.Headers.TryGetValue(HeaderNames.Authorization, out var authHeader);
            await _organizationService.UpdateSupplierAsync(customerId, supplierId, supplierViewModel, username, authHeader);
            return Ok();
        }

        [HttpDelete]
        [AppAuthorize()]
        [Route("{customerId}/suppliers/{supplierId}")]
        public async Task<IActionResult> RemoveSupplierRelationship(long customerId, long supplierId)
        {
            var username = CurrentUser.Username;
            await _organizationService.RemoveSupplierAsync(customerId, supplierId, username);
            return Ok();
        }


        [HttpGet]
        [AppAuthorize()]
        [Route("principals")]
        public async Task<IActionResult> GetPrincipalDataSource(long organizationId, long roleId, string affiliates, bool checkIsBuyer = true)
        {
            var viewModels = await _organizationService.GetPrincipalSelectionsAsync(CurrentUser.IsInternal, roleId, organizationId, affiliates, checkIsBuyer);

            return new JsonResult(viewModels);
        }

        [HttpGet]
        [AppAuthorize()]
        [Route("{customerId}/supplierSelections")]
        public async Task<IActionResult> GetSupplierDataSource(long customerId)
        {
            var viewModels = await _organizationService.GetSupplierSelectionsByPrincipalAsync(customerId);

            return new JsonResult(viewModels);
        }

        [HttpGet]
        [AppAuthorize()]
        [Route("SelectionsByOrgType")]
        public async Task<IActionResult> GetSelectionByOrgTypeAsync([FromQuery] OrganizationType type)
        {
            var viewModels = await _organizationService.GetSelectionsAsync(type);

            return new JsonResult(viewModels);
        }

        /// <summary>
        /// To get list of organization ids which user can be access on reports
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="roleId"></param>
        /// <param name="affiliates"></param>
        /// <returns></returns>
        [HttpGet]
        [AppAuthorize()]
        [Route("Reports/AccessibleOrganizationIds")]
        public async Task<IActionResult> GetAccessibleOrganizationIdsForReport(long organizationId, long roleId, string affiliates)
        {
            var viewModels = await _organizationService.GetAccessibleOrganizationIdsForReportAsync(roleId, organizationId, affiliates);
            return new JsonResult(viewModels);
        }

        #endregion

        [HttpGet("{id}/CheckValidReportPrincipal")]
        public async Task<IActionResult> CheckValidReportPrincipalAsync(long id, Role roleId, long requestingOrganizationId)
        {
            var isValidReportPrincipal = await _organizationService.ValidateReportPrincipal(id, roleId, requestingOrganizationId);
            return new JsonResult(new { IsValidReportPrincipal = isValidReportPrincipal });
        }

        [HttpGet("Agents")]
        public async Task<IActionResult> CheckAgentDomainAsync(string domain)
        {
            if (string.IsNullOrEmpty(domain))
            {
                return new JsonResult(false);
            }
            var result = await _organizationService.CheckAgentDomainAsync(domain);
            return new JsonResult(result);
        }

        [HttpGet("Agents/DropDown")]
        public async Task<IActionResult> GetAgentOrganizationDropDownListAsync()
        {
            var result = await _organizationService.GetAgentOrganizationDropDownListAsync();
            return new JsonResult(result);
        }

        /// <summary>
        /// To search organizations by name to switch user role via GUI
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        [HttpGet("UserRoleSwitchMode/Selections")]
        public async Task<IActionResult> GetSwitchOrganizationSelectionsAsync(string searchTerm)
        {
            var result = await _organizationService.GetSwitchOrganizationSelectionsAsync(searchTerm);
            return new JsonResult(result);
        }
    }
}
