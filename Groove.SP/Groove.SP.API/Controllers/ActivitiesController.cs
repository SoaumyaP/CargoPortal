using Groove.SP.Application.Activity.Services.Interfaces;
using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
    public class ActivitiesController : ControllerBase
    {
        public readonly IActivityService _activityService;

        public ActivitiesController(IActivityService activityService)
        {
            _activityService = activityService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(long id)
        {
            var result = await _activityService.GetAsync(id);
            return new JsonResult(result);
        }

        [HttpPost]
        
        public async Task<IActionResult> PostAsync([FromBody]ActivityViewModel model)
        {
            model.Audit(CurrentUser.Username);
            var result = await _activityService.CreateAsync(model);
            return Ok(result);
        }


        [HttpPut("{id}")]
        
        public async Task<IActionResult> PutAsync(long id, [FromBody]ActivityViewModel model)
        {
            model.Audit(CurrentUser.Username);
            var result = await _activityService.UpdateAsync(model, id);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        
        public async Task<IActionResult> DeleteAsync(long id)
        {
            await _activityService.DeleteAsync(id);
            return Ok();
        }
    }
}
