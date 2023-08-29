using System.Threading.Tasks;
using Groove.CSFE.Application.Authorization;
using Groove.CSFE.Application.Organizations.Services.Interfaces;
using Groove.CSFE.Application.Organizations.ViewModels;
using Groove.CSFE.CommonAPI.Filters;
using Groove.CSFE.Core.Data;
using Microsoft.AspNetCore.Mvc;

namespace Groove.CSFE.CommonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize]
    public class SupplierRelationshipsController : ControllerBase
    {
        private readonly IOrganizationListService _organizationListService;
        private readonly ICustomerRelationshipService _customerRelationshipService;

        public SupplierRelationshipsController(IOrganizationListService organizationListService,
            ICustomerRelationshipService customerRelationshipService)
        {
            _organizationListService = organizationListService;
            _customerRelationshipService = customerRelationshipService;
        }
        
        [HttpGet]
        [Route("Search")]
        public async Task<IActionResult> Search([DataSourceRequest]DataSourceRequest request, long id)
        {
            var viewModels = await _organizationListService.GetListSupplierRelationshipAsync(request, id);
            return new JsonResult(viewModels);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromQuery]long customerId, [FromQuery]long supplierId, [FromBody] CustomerRelationshipViewModel model)
        {
            await _customerRelationshipService.UpdateAsync(customerId, supplierId, model, CurrentUser.Username);
            return Ok();
        }
    }
}
