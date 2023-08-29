using System.Threading.Tasks;
using Groove.CSFE.Application.Authorization;
using Groove.CSFE.Application.OrganizationRoles.Services.Interfaces;
using Groove.CSFE.Application.OrganizationRoles.ViewModels;
using Groove.CSFE.Core;
using Microsoft.AspNetCore.Mvc;

namespace Groove.CSFE.CommonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class OrganizationRolesController : ControllerBase
    {
        private readonly IOrganizationRoleService _organizationRoleService;

        public OrganizationRolesController(IOrganizationRoleService organizationRolesService)
        {
            _organizationRoleService = organizationRolesService;
        }

        [HttpGet]
        //TODO:
        //[AppAuthorize(AppPermissions.PO_Detail_Edit, AppPermissions.PO_Fulfillment_Detail_Edit)]
        [AppAuthorize]
        public async Task<IActionResult> GetAllRoles()
        {
            var viewModels = await _organizationRoleService.GetAllAsync();
            return new JsonResult(viewModels);
        }

        [HttpPost]
        
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
        public async Task<IActionResult> PostAsync([FromBody]OrganizationRoleViewModel model)
        {
            var result = await _organizationRoleService.CreateAsync(model);
            return Ok(result);
        }

        [HttpPut("{id}")]
        
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
        public async Task<IActionResult> UpdateAsync(long id, [FromBody]OrganizationRoleViewModel model)
        {
            var result = await _organizationRoleService.UpdateAsync(model, id);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
        public async Task<IActionResult> DeleteAsync(long id)
        {
            var result = await _organizationRoleService.DeleteAsync(id);

            return Ok(result);
        }
    }
}
