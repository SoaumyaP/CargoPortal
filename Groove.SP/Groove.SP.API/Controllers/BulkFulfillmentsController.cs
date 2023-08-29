using Microsoft.AspNetCore.Mvc;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.BulkFulfillment.Services.Interfaces;
using System.Threading.Tasks;
using Groove.SP.Application.BulkFulfillment.ViewModels;
using Groove.SP.Core.Models;
using Groove.SP.Application.Attachment.Services.Interfaces;
using System.Linq;
using Groove.SP.Application.Note.Services.Interfaces;
using Groove.SP.Application.GlobalIdMasterDialog.Services.Interfaces;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities;
using Groove.SP.API.Filters;
using Groove.SP.Core.Data;
using Groove.SP.Application.ViewSetting.Services.Interfaces;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize]
    public class BulkFulfillmentsController : ControllerBase
    {
        private readonly IBulkFulfillmentService _bulkFulfillmentService;
        private readonly IAttachmentService _attachmentService;
        private readonly INoteService _noteService;
        private readonly IGlobalIdMasterDialogService _globalIdMasterDialogService;
        private readonly IViewSettingService _viewSettingService;

        public BulkFulfillmentsController(
            IBulkFulfillmentService bulkFulfillmentService,
            IAttachmentService attachmentService,
            INoteService noteService,
            IViewSettingService viewSettingService,
            IGlobalIdMasterDialogService globalIdMasterDialogService)
        {
            _bulkFulfillmentService = bulkFulfillmentService;
            _attachmentService = attachmentService;
            _noteService = noteService;
            _viewSettingService = viewSettingService;
            _globalIdMasterDialogService = globalIdMasterDialogService;
        }

        [HttpGet]
        [Route("search")]
        public async Task<IActionResult> SearchAsync([DataSourceRequest] DataSourceRequest request, string affiliates, long? organizationId = 0)
        {
            var viewModels = await _bulkFulfillmentService.SearchAsync(request, CurrentUser.IsInternal, affiliates, organizationId);

            // Toggle field response by current user role
            var vwSettings = await _viewSettingService.ApplyViewSettingsAsync(viewModels.Data, request.ViewSettingModuleId ?? ViewSettingModuleId.BULKBOOKING_COPY_LIST, CurrentUser.UserRoleId);
            // Respond back to the client for GUI rendering purposes.
            viewModels.ViewSettings = vwSettings;

            return new JsonResult(viewModels);
        }

        [HttpPost("{id}/bookingRequests")]
        [AppAuthorize(AppPermissions.PO_Fulfillment_Detail_Edit)]
        public async Task<IActionResult> SubmitBookingAsync(long id)
        {
            var poff = await _bulkFulfillmentService.SubmitBookingAsync(id, CurrentUser);
            return Ok(poff);
        }

        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmBookingAsync(EdiSonConfirmPOFFViewModel viewModel)
        {
            await _bulkFulfillmentService.ConfirmBookingAsync(viewModel);
            return Ok();
        }

        [HttpPut("{id}/amend")]
        [AppAuthorize(AppPermissions.PO_Fulfillment_Detail_Edit)]
        public async Task<IActionResult> AmendBulkBookingAsync(long id)
        {
            await _bulkFulfillmentService.AmendAsync(id, CurrentUser.Username);

            return Ok();
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get(long id, string affiliates, string formType)
        {
            var viewModel = await _bulkFulfillmentService.GetAsync(id, CurrentUser.IsInternal, affiliates);
            if (viewModel != null)
            {
                // Load attachments depending on Attachment Type Classification/Permission
                viewModel.Attachments = (await _attachmentService.GetAttachmentsCrossModuleAsync(EntityType.POFullfillment, id, CurrentUser.UserRoleId, CurrentUser.OrganizationId)).ToList();

                if (formType == FormModeType.VIEW)
                {
                    // Toggle field response by current user role
                    await _viewSettingService.ApplyViewSettingsAsync(viewModel, CurrentUser.UserRoleId);
                }
                
                return new JsonResult(viewModel);
            }

            return new JsonResult(viewModel);
        }

        [HttpGet]
        [Route("{id}/planned-schedule")]
        public async Task<IActionResult> GetPlannedScheduleAsync(long id)
        {
            var viewModel = await _bulkFulfillmentService.GetPlannedScheduleAsync(id);

            // Toggle field response by current user role
            viewModel.ViewSettingModuleId = ViewSettingModuleId.BULKBOOKING_DETAIL_PLANNED_SCHEDULE;
            await _viewSettingService.ApplyViewSettingsAsync(viewModel, CurrentUser.UserRoleId);
            return new JsonResult(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync(InputBulkFulfillmentViewModel viewModel, [FromQuery] bool updateOrganizationPreferences, [FromQuery] bool updateOrgContactPreferences)
        {
            viewModel.Audit(null, CurrentUser.Username);
            viewModel.UpdateOrganizationPreferences = updateOrganizationPreferences;
            viewModel.UpdateOrgContactPreferences = updateOrgContactPreferences;
            var result = await _bulkFulfillmentService.CreateAsync(viewModel, CurrentUser);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync([FromBody] InputBulkFulfillmentViewModel viewModel, long id, [FromQuery] bool updateOrganizationPreferences, [FromQuery] bool updateOrgContactPreferences)
        {
            viewModel.Audit(null, CurrentUser.Username);
            viewModel.UpdateOrganizationPreferences = updateOrganizationPreferences;
            viewModel.UpdateOrgContactPreferences = updateOrgContactPreferences;
            var result = await _bulkFulfillmentService.UpdateAsync(viewModel, CurrentUser);
            return Ok(result);
        }

        [HttpPut("{id}/cancel")]
        [AppAuthorize(AppPermissions.PO_Fulfillment_Detail_Edit)]
        public async Task<IActionResult> CancelBulkFulfillmentAsync(long id, CancelBulkFulfillmentViewModel cancelModel)
        {
            var poff = await _bulkFulfillmentService.CancelAsync(id, CurrentUser.Username, cancelModel);

            return Ok(poff);
        }

        [HttpGet("{id}/notes")]
        public async Task<IActionResult> GetNotesByPOFulfillment(long id)
        {
            var result = await _noteService.GetPOFulfillmentNotesByIdAsync(id);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{id}/MasterDialogs")]
        public async Task<IActionResult> GetMasterDialogsByPOFulfillment(long id)
        {
            var result = await _globalIdMasterDialogService.GetByPOFulfillmentAsync(id);
            return new JsonResult(result);
        }

        [HttpPut("{id}/planToShip")]
        [AppAuthorize(AppPermissions.PO_Fulfillment_Detail_Edit)]
        public async Task<IActionResult> PlanToShip(long id)
        {
            await _bulkFulfillmentService.PlanToShipAsync(id, CurrentUser.Username);
            return Ok();
        }

        /// <summary>
        /// Re-load means plan to ship again.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("{id}/re-load")]
        [AppAuthorize(AppPermissions.PO_Fulfillment_Detail_Edit)]
        public async Task<IActionResult> ReloadAsync(long id, [FromBody] InputBulkFulfillmentViewModel model)
        {
            model.ValidateAndThrow();
            await _bulkFulfillmentService.ReloadAsync(id, model, CurrentUser);
            return Ok();
        }
    }
}