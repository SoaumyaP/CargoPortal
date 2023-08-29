using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.BuyerComplianceService.Services.Interfaces;
using Groove.SP.Application.BuyerCompliance.ViewModels;
using Groove.SP.Core.Models;
using Groove.SP.Application.BuyerCompliance.Services.Interfaces;
using Groove.SP.Core.Data;
using Groove.SP.API.Filters;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuyerCompliancesController : ControllerBase
    {
        private readonly IBuyerComplianceService _buyerComplianceService;

        private readonly IBuyerComplianceListService _buyerComplianceListService;

        public BuyerCompliancesController(
            IBuyerComplianceService buyerComplianceService,
            IBuyerComplianceListService buyerComplianceListService)
        {
            _buyerComplianceService = buyerComplianceService;
            _buyerComplianceListService = buyerComplianceListService;
        }
        
        [HttpGet]
        [Route("search")]
        [AppAuthorize(AppPermissions.Organization_Compliance_List)]
        public async Task<IActionResult> Search([DataSourceRequest] DataSourceRequest request)
        {
            var viewModels = await _buyerComplianceListService.ListAsync(request);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("{id}/isOrganizationExists/{organizationId}")]
        [AppAuthorize(AppPermissions.Organization_Compliance_Detail)]
        public async Task<IActionResult> IsOrganizationExists(long id, long organizationId)
        {
            var result = await _buyerComplianceService.IsOrganizationExists(id, organizationId);
            return new JsonResult(result);
        }

        [HttpGet]
        //TODO: 
        //[AppAuthorize(AppPermissions.Organization_Compliance_Detail)]
        public async Task<IActionResult> Get([FromQuery] long? organizationId)
        {
            var viewModels = await _buyerComplianceService.GetListAsync(organizationId);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("{id}")]
        [AppAuthorize(AppPermissions.Organization_Compliance_Detail)]
        public async Task<IActionResult> Get(long id)
        {
            var viewModels = await _buyerComplianceService.GetAsync(id);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("dropdown/hasEnableProgressCheck/{roleId}")]
        public async Task<IActionResult> GetDropDownByProgressCheckStatusAsync(int roleId, string affiliates)
        {
            var viewModels = await _buyerComplianceService.GetDropDownByProgressCheckStatusAsync(roleId,affiliates,true);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("dropdown/warehouse-service-type")]
        public async Task<IActionResult> GetWarehouseServiceTypeDropDownAsync()
        {
            var viewModels = await _buyerComplianceService.GetWarehouseServiceTypeDropDownAsync();
            return new JsonResult(viewModels);
        }

        #region Create / Edit
        [HttpPost]
        [AppAuthorize(AppPermissions.Organization_Compliance_Detail_Edit)]
        public async Task<IActionResult> PostAsync([FromBody] SaveBuyerComplianceViewModel model)
        {
            model.ValidateAndThrow();
            model.Audit(CurrentUser.Username);
            var result = await _buyerComplianceService.CreateAsync(model);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [AppAuthorize(AppPermissions.Organization_Compliance_Detail_Edit)]
        public async Task<IActionResult> PutAsync(long id, [FromBody] SaveBuyerComplianceViewModel model)
        {
            model.ValidateAndThrow();
            model.Audit(CurrentUser.Username);
            var result = await _buyerComplianceService.UpdateAsync(model);
            return Ok(result);
        }

        [HttpPut("{id}/activate")]
        [AppAuthorize(AppPermissions.Organization_Compliance_Detail_Edit)]
        public async Task<IActionResult> ActivateAsync(long id, [FromBody] ActivateBuyerComplianceViewModel model)
        {
            model.ValidateAndThrow();
            model.Audit(CurrentUser.Username);
            var result = await _buyerComplianceService.ActivateAsync(model);
            return Ok(result);
        }

        [HttpGet]
        [AppAuthorize()]
        [Route("principals/agent")]
        public async Task<IActionResult> GetPrincipalsForAgent(long organizationId)
        {
            var viewModels = await _buyerComplianceService.GetPrincipalSelectionsForAgentRoleAsync(organizationId);

            return new JsonResult(viewModels);
        }
        #endregion
    }
}
