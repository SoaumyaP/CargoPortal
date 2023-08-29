using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Groove.CSFE.Application.Authorization;
using Groove.CSFE.Application.EventCodes.Services.Interfaces;
using Groove.CSFE.Application.EventCodes.ViewModels;
using Groove.CSFE.CommonAPI.Filters;
using Groove.CSFE.Core;
using Groove.CSFE.Core.Data;
using Microsoft.AspNetCore.Mvc;

namespace Groove.CSFE.CommonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventCodesController : ControllerBase
    {
        private readonly IEventCodeService _eventCodeService;
        private readonly IEventCodeListService _eventCodeListService;

        public EventCodesController(IEventCodeService eventCodeService, IEventCodeListService eventCodeListService)
        {
            _eventCodeService = eventCodeService;
            _eventCodeListService = eventCodeListService;
        }

        [HttpGet("search")]
        [AppAuthorize(AppPermissions.Organization_MasterEvent_List)]
        public async Task<IActionResult> SearchAsync([DataSourceRequest] DataSourceRequest request)
        {
            var res = await _eventCodeListService.SearchAsync(request);
            return Ok(res);
        }

        [HttpPost]
        [AppAuthorize(AppPermissions.Organization_MasterEvent_List_Add)]
        public async Task<IActionResult> CreateAsync(CreateEventCodeViewModel viewModel)
        {
            var result = await _eventCodeService.CreateAsync(viewModel, CurrentUser.Username);
            return Ok(result);
        }

        [HttpPut]
        [AppAuthorize(AppPermissions.Organization_MasterEvent_List_Edit)]
        public async Task<IActionResult> UpdateAsync(CreateEventCodeViewModel viewModel)
        {
            var result = await _eventCodeService.UpdateAsync(viewModel, CurrentUser.Username);
            return Ok(result);
        }

        [HttpPut("sequenceUpdates")]
        [AppAuthorize]
        [AppAuthorize(AppPermissions.Organization_MasterEvent_List_Edit)]
        public async Task<IActionResult> UpdateSequenceAsync(List<UpdateEventSequenceViewModel> model)
        {
            await _eventCodeService.UpdateSequenceAsync(model, CurrentUser.Username);
            return Ok();
        }

        [HttpPut("{activityCode}/statusUpdate")]
        [AppAuthorize(AppPermissions.Organization_MasterEvent_List)]
        public async Task<IActionResult> UpdateStatusAsync([FromBody] UpdateEventStatusViewModel vm, string activityCode)
        {
            await _eventCodeService.UpdateStatusAsync(activityCode, vm, CurrentUser.Username);
            return Ok();
        }

        [HttpGet]
        [AppAuthorize]
        public async Task<IActionResult> GetEventByCode(string code)
        {
            if (!string.IsNullOrWhiteSpace(code))
            {
                var viewModel = await _eventCodeService.GetByCodeAsync(code);
                return Ok(viewModel);
            }

            var viewModels = await _eventCodeService.GetAllAsync();
            return new JsonResult(viewModels);
        }

        [HttpGet("listByCodes")]
        [AppAuthorize]
        public async Task<IActionResult> GetEventByCodes([FromQuery] string codes)
        {
            var codeList = codes?.Split(',').Select(x => x.ToString().Trim());
            if (codeList == null || !codeList.Any())
            {
                codeList = new List<string>();
            }
            var viewModels = await _eventCodeService.GetByCodesAsync(codeList);
            return new JsonResult(viewModels);
        }

        [HttpGet("eventByTypes")]
        [AppAuthorize]
        public async Task<IActionResult> GetEventByTypes(string types)
        {
            var viewModels = await _eventCodeService.GetByTypeAsync(types);
            return new JsonResult(viewModels);
        }

        [HttpGet("eventByLevel")]
        [AppAuthorize]
        public async Task<IActionResult> GetEventByLevel(int level)
        {
            var viewModels = await _eventCodeService.GetByLevelAsync(level);
            return new JsonResult(viewModels);
        }

        [HttpGet("dropdown")]
        [AppAuthorize]
        public async Task<IActionResult> GetDropdownAsync()
        {
            var viewModels = await _eventCodeService.GetDropdownListAsync();
            return new JsonResult(viewModels);
        }

        [HttpGet("check-already-exist")]
        [AppAuthorize]
        public async Task<IActionResult> CheckEventCodeAlreadyExistAsync(string code)
        {
            var viewModels = await _eventCodeService.CheckEventCodeAlreadyExistAsync(code);
            return new JsonResult(viewModels);
        }
    }
}
