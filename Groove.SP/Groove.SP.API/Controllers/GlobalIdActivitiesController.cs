using Groove.SP.Application.Authorization;
using Groove.SP.Application.GlobalIdActivity.RequestModels;
using Groove.SP.Application.GlobalIdActivity.Services.Interfaces;
using Groove.SP.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize]
    public class GlobalIdActivitiesController : ControllerBase
    {
        private readonly IGlobalIdActivityService _globalIdActivityService;

        public GlobalIdActivitiesController(IGlobalIdActivityService globalIdActivityService)
        {
            _globalIdActivityService = globalIdActivityService;
        }

        /// <summary>
        /// To get data for activity time line
        /// </summary>
        [HttpGet]
        [Route("activities-timeline")]
        public IActionResult GetActivitiesTimelineAsync([FromQuery] GetActivityTimelineRequestModel requestModel)
        {
            var result = _globalIdActivityService.GetActivityTimelineAsync(requestModel.EntityId, requestModel.EntityType, requestModel);

            return Ok(result);
        }

        /// <summary>
        /// To get data for activity time line on booking module
        /// </summary>
        [HttpGet]
        [Route("get-by-poff/{poffId}")]
        public IActionResult GetTimelineByPOFFAsync(long poffId, [FromQuery] GetActivityTimelineRequestModel requestModel)
        {
            var result = _globalIdActivityService.GetActivityTimelineAsync(poffId, EntityType.POFullfillment, requestModel);

            return Ok(result);
        }


        /// <summary>
        /// To get data for activity time line on purchase order module
        /// </summary>
        [HttpGet]
        [Route("get-by-po/{poId}")]
        public IActionResult GetTimelineByPOAsync(long poId, [FromQuery] GetActivityTimelineRequestModel requestModel)
        {
            var result = _globalIdActivityService.GetActivityTimelineAsync(poId, EntityType.CustomerPO, requestModel);
            return Ok(result);
        }

        /// <summary>
        /// To get the total activity of the booking.
        /// </summary>
        [HttpGet]
        [Route("get-total-by-poff/{poffId}")]
        public async Task<IActionResult> GetTotalByPOFFAsync(long poffId)
        {
            var result = await _globalIdActivityService.GetActivityTotalAsync(poffId, EntityType.POFullfillment);
            return Ok(result);
        }

        /// <summary>
        /// To get filter value dropdown datasource for activity timeline.
        /// </summary>
        [HttpGet]
        [Route("filter-value-dropdown")]
        public async Task<IActionResult> GetFilterValueDropdownAsync([FromQuery]long entityId, [FromQuery]string entityType, [FromQuery]string filterBy)
        {
            var result = await _globalIdActivityService.GetFilterValueDropdownAsync(entityId, entityType, filterBy);
            return Ok(result);
        }
    }
}