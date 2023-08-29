using Groove.SP.Application.Authorization;
using Groove.SP.Application.OrgContactPreference.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize]
    public class OrgContactPreferencesController : ControllerBase
    {
        private readonly IOrgContactPreferenceService _orgContactPreferenceService;

        public OrgContactPreferencesController(
            IOrgContactPreferenceService orgContactPreferenceService
            )
        {
            _orgContactPreferenceService = orgContactPreferenceService;
        }

        [HttpGet("organization/{organizationId}")]
        public async Task<IActionResult> GetAsync(long organizationId)
        {
            var viewModels = await _orgContactPreferenceService.GetAsNoTrackingAsync(organizationId);
            return new JsonResult(viewModels);
        }
    }
}
