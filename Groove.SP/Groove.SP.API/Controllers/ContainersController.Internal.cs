using Groove.SP.Application.Activity.Services.Interfaces;
using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.Container.Services.Interfaces;
using Groove.SP.Application.Container.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Groove.SP.API.Controllers
{
    /// <summary>
    /// To be used by CS Portal. Url is /api/containers/internal/
    /// </summary>
    [Route("api/containers/internal")]
    [ApiController]
    public class InternalContainersController : ControllerBase
    {
        private readonly IContainerService _containerService;
        private readonly IActivityService _activityService;
        public InternalContainersController(
            IContainerService containerService,
            IActivityService activityService)
        {
            _containerService = containerService;
            _activityService = activityService;
        }

        [HttpPut]
        [Route("{id}")]
        [AppAuthorize(AppPermissions.Shipment_ContainerDetail_Edit)]
        public async Task<IActionResult> PutAsync(long id, UpdateContainerViaUIViewModel model)
        {
            var result = await _containerService.UpdateAsync(id, model, CurrentUser.Username);
            return new JsonResult(result);
        }

        [HttpPost("{id}/activities")]
        [AppAuthorize(AppPermissions.Shipment_ContainerDetail)]
        public async Task<IActionResult> PostActivityAsync(long id, [FromBody] ActivityViewModel model)
        {
            model.Audit(CurrentUser.Username);
            var result = await _activityService.CreateAsync(model);
            return Ok(result);
        }

        /// <summary>
        /// Call to check duplicate on a pair of container number and carrier so number.
        /// If check to add new => id = 0.
        /// If check to update container => id = checking containerId.
        /// </summary>
        /// <param name="containerNo"></param>
        /// <param name="carrierSONo"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/checkDuplicateContainer")]
        public async Task<IActionResult> CheckDuplicateContainerAsync(long id, string containerNo, string carrierSONo)
        {
            var result = await _containerService.IsDuplicatedContainerAsync(containerNo, carrierSONo, id);
            return Ok(result);
        }


        [HttpPut("{id}/activities/{activityId}")]
        [AppAuthorize(AppPermissions.Shipment_ContainerDetail)]
        public async Task<IActionResult> PutActivityAsync(long id, long activityId, [FromBody] ActivityViewModel model)
        {
            model.Audit(CurrentUser.Username);
            var result = await _activityService.UpdateAsync(model, activityId);
            return Ok();
        }

        [HttpDelete("{id}/activities/{activityId}")]
        [AppAuthorize(AppPermissions.Shipment_ContainerDetail)]
        public async Task<IActionResult> DeleteActivityAsync(long activityId)
        {
            await _activityService.DeleteAsync(activityId);
            return Ok();
        }
    }
}
