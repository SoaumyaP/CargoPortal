using AutoMapper;
using Groove.SP.API.Filters;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.Provider.Report;
using Groove.SP.Application.Scheduling.Services.Interfaces;
using Groove.SP.Application.Scheduling.ViewModels;
using Groove.SP.Application.Translations.Providers.Interfaces;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class SchedulingsController : ControllerBase
    {
        private readonly ISchedulingListService _schedulingListService;
        private readonly ISchedulingService _schedulingService;
        private readonly ITelerikReportProvider _telerikReportProvider;
        private readonly IMapper _mapper;
        private readonly ITranslationProvider _translation;

        public SchedulingsController(
            ISchedulingListService schedulingListService,
            ISchedulingService schedulingService,
            ITelerikReportProvider telerikReportProvider,
            IMapper mapper,
            ITranslationProvider translation
        )
        {
            _schedulingListService = schedulingListService;
            _schedulingService = schedulingService;
            _telerikReportProvider = telerikReportProvider;
            _mapper = mapper;
            _translation = translation;
    }

        [HttpGet]
        [Route("search")]
        [AppAuthorize(AppPermissions.Reports_List)]
        public async Task<IActionResult> GetSchedulingList([DataSourceRequest] DataSourceRequest request)
        {

            if (request.Sorts == null)
            {
                request.Sorts = new[]
                {
                    new SortDescriptor (nameof(SchedulingQueryModel.UpdatedDate), ListSortDirection.Descending)
                };
            };

            var viewModels = await _schedulingListService.ListAsync(request, CurrentUser.IsInternal, CurrentUser.OrganizationId);

            var data = viewModels.Data as IList<SchedulingQueryModel>;
            foreach (var scheduling in data)
            {
                scheduling.StatusName = await _translation.GetTranslationByKeyAsync(EnumHelper<SchedulingStatus>.GetDisplayName(scheduling.Status));
            }


            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("{id}")]
        [AppAuthorize(AppPermissions.Reports_TaskDetail)]
        public async Task<IActionResult> GetScheduling(long id)
        {
            if (id == 0)
            {
                return new JsonResult(null);
            }

            var scheduling = await _schedulingService.GetSchedulingAsync(id, CurrentUser.IsInternal, CurrentUser.OrganizationId);

            return new JsonResult(scheduling);
        }

        [HttpPost]
        [AppAuthorize(AppPermissions.Reports_TaskDetail_Add)]
        public async Task<IActionResult> CreateNewScheduling(SchedulingViewModel data)
        {
            data.CreatedOrganizationId = CurrentUser.OrganizationId;
            data.ValidateAndThrow(false);

            var newScheduling = await _schedulingService.CreateSchedulingAsync(data, CurrentUser);

            // Get data for the created scheduling
            var result = await _schedulingService.GetSchedulingAsync(newScheduling.Id, CurrentUser.IsInternal, CurrentUser.OrganizationId);
            return new JsonResult(result);
        }

        [HttpPut]
        [AppAuthorize(AppPermissions.Reports_TaskDetail_Edit)]
        public async Task<IActionResult> UpdateScheduling(SchedulingViewModel data)
        {
            data.ValidateAndThrow(false);

            await _schedulingService.UpdateSchedulingAsync(data, CurrentUser.IsInternal, CurrentUser.OrganizationId, CurrentUser.Email);

            // Get the latest data of scheduling
            var result = await _schedulingService.GetSchedulingAsync(data.Id, CurrentUser.IsInternal, CurrentUser.OrganizationId);
            return new JsonResult(result);
        }

        [HttpDelete]
        [Route("{id}")]
        [AppAuthorize(AppPermissions.Reports_TaskDetail)]
        public async Task<IActionResult> DeleteScheduling(long id, string telerikTaskId)
        {
            if (id == 0 || string.IsNullOrEmpty(telerikTaskId))
            {
                return BadRequest();
            }

            await _schedulingService.DeleteSchedulingAsync(id, CurrentUser.IsInternal, CurrentUser.OrganizationId);

            return new JsonResult(null);
        }

        [HttpPut]
        [Route("{id}/Subscribers")]
        [AppAuthorize(AppPermissions.Reports_TaskDetail_Edit)]
        public async Task<IActionResult> SetSubscribers(long id, string telerikTaskId, string[] emails)
        {
            if (id == 0 || string.IsNullOrEmpty(telerikTaskId))
            {
                return BadRequest();
            }

            await _telerikReportProvider.SetSubscribersAsync(telerikTaskId, emails);

            return new JsonResult(null);
        }

        [HttpDelete]
        [Route("{id}/Subscribers")]
        [AppAuthorize(AppPermissions.Reports_TaskDetail_Edit)]
        public async Task<IActionResult> RemoveSubscriber(long id, string telerikTaskId, string telerikUserId, string email)
        {
            if (id == 0 || string.IsNullOrEmpty(telerikTaskId) || string.IsNullOrEmpty(email))
            {
                return BadRequest();
            }

            // Remove external user
            if (telerikUserId == email)
            {
                await _telerikReportProvider.RemoveSubscriberAsync(telerikTaskId, email);
            }
            else
            {
                // Remove internal user
                await _telerikReportProvider.RemoveSubscriberAsync(telerikTaskId, telerikUserId);

            }

            return new JsonResult(null);
        }

        [HttpDelete]
        [Route("{id}/Activities")]
        [AppAuthorize(AppPermissions.Reports_TaskDetail_Edit)]
        public async Task<IActionResult> RemoveActivity(long id, string telerikTaskId, string telerikDocumentId)
        {
            if (id == 0 || string.IsNullOrEmpty(telerikTaskId) || string.IsNullOrEmpty(telerikDocumentId))
            {
                return BadRequest();
            }
            // Remove internal user
            await _telerikReportProvider.RemoveActivityAsync(telerikDocumentId, telerikTaskId);

            return new JsonResult(null);
        }

        [HttpPut]
        [Route("{id}/Deactivate")]
        [AppAuthorize(AppPermissions.Reports_TaskDetail_Edit)]
        public async Task<IActionResult> DeactivateScheduling(long id)
        {
            if (id == 0)
            {
                return BadRequest();
            }

            await _schedulingService.UpdateSchedulingStatusAsync(id, Core.Models.SchedulingStatus.Inactive, CurrentUser.IsInternal, CurrentUser.OrganizationId, CurrentUser.Username);

            return new JsonResult(null);
        }

        [HttpPut]
        [Route("{id}/Activate")]
        [AppAuthorize(AppPermissions.Reports_TaskDetail_Edit)]
        public async Task<IActionResult> ActivateScheduling(long id)
        {
            if (id == 0)
            {
                return BadRequest();
            }

            await _schedulingService.UpdateSchedulingStatusAsync(id, Core.Models.SchedulingStatus.Active, CurrentUser.IsInternal, CurrentUser.OrganizationId, CurrentUser.Username);

            return new JsonResult(null);
        }

        [HttpGet]
        [Route("{id}/ExecuteTask")]
        [AppAuthorize(AppPermissions.Reports_TaskDetail)]
        public async Task<IActionResult> ExecuteScheduling(long id, string telerikTaskId)
        {
            if (id == 0 || string.IsNullOrEmpty(telerikTaskId))
            {
                return BadRequest();
            }

            await _telerikReportProvider.ExecuteTaskAsync(telerikTaskId);

            return new JsonResult(null);
        }

    }
}
