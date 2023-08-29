using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.OrganizationPreference.Services.Interfaces;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize]
    public class OrganizationPreferencesController : ControllerBase
    {
        private readonly IOrganizationPreferenceService _organizationPreferenceService;

        public OrganizationPreferencesController(
            IOrganizationPreferenceService organizationPreferenceService
            )
        {
            _organizationPreferenceService = organizationPreferenceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync(long organizationId, string productCode)
        {
            var viewModels = await _organizationPreferenceService.GetAsNoTrackingAsync(organizationId, productCode);
            return new JsonResult(viewModels);
        }

        [HttpGet("organization/{organizationId}")]
        public async Task<IActionResult> GetByOrganizationAsync(long organizationId)
        {
            var viewModels = await _organizationPreferenceService.GetAsNoTrackingAsync(organizationId);
            return new JsonResult(viewModels);
        }
    }
}
