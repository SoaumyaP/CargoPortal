using Groove.SP.Application.Activity.Services.Interfaces;
using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.Authorization;
using Groove.SP.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize(Scope = "spapi.updateactivities")]
    public class ActivityUpdateController : ControllerBase
    {
        private readonly IActivityService _activityService;
        public ActivityUpdateController(IActivityService activityService)
        {
            _activityService = activityService;
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] AgentActivityCreateViewModel model)
        {
            model.AuditForAPI(string.Empty, false);
            var result = await _activityService.CreateAsync(model);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> PutAsync([FromBody] AgentActivityUpdateViewModel model)
        {
            model.AuditForAPI(string.Empty, true);
            model.FieldStatus[nameof(AgentActivityUpdateViewModel.UpdatedDate)] = FieldDeserializationStatus.HasValue;

            var result = await _activityService.UpdateAsync(model);
            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromBody] AgentActivityDeleteViewModel model)
        {
            await _activityService.DeleteAsync(model);
            return NoContent();
        }
    }
}
