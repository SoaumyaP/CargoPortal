using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.POFulfillment.Services.Interfaces;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Application.Attachment.Services.Interfaces;
using System.Linq;
using Groove.SP.Application.Providers.BlobStorage;
using System.Collections.Generic;
using Groove.SP.Core.Models;
using System;
using Groove.SP.Application.Attachment.ViewModels;
using Groove.SP.Application.Note.Services.Interfaces;
using Groove.SP.Application.GlobalIdMasterDialog.Services.Interfaces;
using Groove.SP.Application.POFulfillment.Validations;
using FluentValidation;
using Groove.SP.Application.WarehouseFulfillment.Services.Interfaces;
using Groove.SP.Infrastructure.CSFE;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.BuyerComplianceService.Services.Interfaces;
using Groove.SP.Core.Data;
using Groove.SP.API.Filters;
using Groove.SP.Application.ViewSetting.Services.Interfaces;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize]
    public class POFulfillmentsController : ControllerBase
    {
        private readonly IPOFulfillmentService _poFulfillmentService;
        private readonly IPOFulfillmentListService _poFulfillmentListService;
        private readonly IAttachmentService _attachmentService;
        private readonly IBlobStorage _blobStorage;
        private readonly INoteService _noteService;
        private readonly IGlobalIdMasterDialogService _globalIdMasterDialogService;
        private readonly IWarehouseFulfillmentService _warehouseFulfillmentService;
        private readonly IBuyerComplianceService _buyerComplianceService;
        private readonly IViewSettingService _viewSettingService;
        private readonly ICSFEApiClient _csfeApiClient;

        public POFulfillmentsController(IPOFulfillmentService poFulfillmentService,
            IPOFulfillmentListService poFulfillmentListService,
            IAttachmentService attachmentService, IBlobStorage blobStorage,
            INoteService noteService,
            IGlobalIdMasterDialogService globalIdMasterDialogService,
            IWarehouseFulfillmentService warehouseFulfillmentService,
            ICSFEApiClient csfeApiClient,
            IViewSettingService viewSettingService,
            IBuyerComplianceService buyerComplianceService)
        {
            _poFulfillmentService = poFulfillmentService;
            _poFulfillmentListService = poFulfillmentListService;
            _attachmentService = attachmentService;
            _blobStorage = blobStorage;
            _noteService = noteService;
            _globalIdMasterDialogService = globalIdMasterDialogService;
            _warehouseFulfillmentService = warehouseFulfillmentService;
            _csfeApiClient = csfeApiClient;
            _buyerComplianceService = buyerComplianceService;
            _viewSettingService = viewSettingService;
        }

        [HttpGet]
        [Route("statistics/awaiting-for-submission")]
        public async Task<IActionResult> GetBookingAwaitingForSubmissionAsync(long? organizationId = 0, string userRole = "")
        {
            var viewModels = await _poFulfillmentListService.GetBookingAwaitingForSubmissionAsync(organizationId, userRole);
            return new JsonResult(viewModels);
        }

        [HttpPost]
        [Route("{bookingId}/ask-missing-po")]
        public async Task<IActionResult> AskMissingPOAsync(long bookingId, InputPOFulfillmentViewModel viewModel)
        {
             await _poFulfillmentService.AskMissingPOAsync(bookingId,viewModel);
            return Ok();
        }

        [HttpGet]
        [Route("statistics/pending-for-approval")]
        public async Task<IActionResult> GetBookingPendingForApprovalAsync(string affiliates)
        {
            var viewModels = await _poFulfillmentListService.GetBookingPendingForApprovalAsync(affiliates, CurrentUser);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("search")]
        [AppAuthorize(AppPermissions.PO_Fulfillment_List)]
        public async Task<IActionResult> Get([DataSourceRequest] DataSourceRequest request, string affiliates, long? organizationId = 0, string userRole = "", string statisticKey = "")
        {
            var viewModels = await _poFulfillmentListService.GetListPOFulfillmentAsync(request, CurrentUser.IsInternal, affiliates, organizationId, userRole, statisticKey);

            // Toggle field response by current user role
            var vwSettings = await _viewSettingService.ApplyViewSettingsAsync(viewModels.Data, request.ViewSettingModuleId ?? ViewSettingModuleId.BOOKING_LIST, CurrentUser.UserRoleId);

            // Respond back to the client for GUI rendering purposes.
            viewModels.ViewSettings = vwSettings;

            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("{id}")]
        [AppAuthorize(AppPermissions.PO_Fulfillment_Detail)]
        public async Task<IActionResult> Get(long id, string affiliates, string formtype)
        {
            var viewModel = await _poFulfillmentService.GetAsync(id, CurrentUser.IsInternal, affiliates);
            if(viewModel != null)
            {
                // Hide the booking Owner and Delegation Contact
                var allowedOrganizationRoles = new List<string> {OrganizationRole.Shipper, OrganizationRole.Supplier, OrganizationRole.Delegation, OrganizationRole.OriginAgent, OrganizationRole.DestinationAgent};
                var allowedOrganizationIds = viewModel.Contacts?.Where(x => allowedOrganizationRoles.Contains(x.OrganizationRole)).Select(x => x.OrganizationId);

                if(!CurrentUser.IsInternal
                    && allowedOrganizationIds != null
                    && allowedOrganizationIds.Any()
                    && !allowedOrganizationIds.Contains(CurrentUser.OrganizationId))
                {
                    viewModel.CreatedBy = null;
                    viewModel.Contacts = viewModel.Contacts.Where(c => !c.OrganizationRole.Equals(OrganizationRole.Delegation, StringComparison.OrdinalIgnoreCase)).ToList();
                    viewModel.Attachments = Enumerable.Empty<AttachmentViewModel>();
                }
                // Load attachments depending on Attachment Type Classification/Permission
                viewModel.Attachments = (await _attachmentService.GetAttachmentsCrossModuleAsync(EntityType.POFullfillment, id, CurrentUser.UserRoleId, CurrentUser.OrganizationId)).ToList();

                if (formtype?.Trim().ToLower() == FormModeType.VIEW.ToLower())
                {
                    // override default view setting id.
                    _viewSettingService.SetViewSettingModuleId(ViewSettingModuleId.FREIGHTBOOKING_DETAIL_LOAD_DETAILS, viewModel.Loads.SelectMany(x => x.Details));

                    // Toggle field response by current user role
                    await _viewSettingService.ApplyViewSettingsAsync(viewModel, CurrentUser.UserRoleId);
                }

                return new JsonResult(viewModel);
            }
            return new JsonResult(viewModel);
        }

        #region Create / Import / Edit
        /// <summary>
        /// Call to import booking via API.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="profile"></param>
        /// <returns></returns>
        [HttpPost("import")]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
        public async Task<IActionResult> ImportAsync([FromBody] ImportBookingViewModel model, [FromQuery] string profile)
        {
            model.AuditForAPI(CurrentUser.Username, false);

            var isValid = model.Validate(_csfeApiClient, out var errors);
            if (!isValid)
            {
                await _poFulfillmentService.WriteImportingValidationLogAsync(errors, isValid, model, profile);
                return BadRequest(errors);
            }

            var result = await _poFulfillmentService.ImportBookingAsync(model, CurrentUser.Username);
            await _poFulfillmentService.WriteImportingResultLogAsync(result, model, profile);

            if (!result.Success)
            {  
                return BadRequest(result.Result);
            }
            return Ok(result);
        }

        [HttpPut("confirmUpdates")]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
        public async Task<IActionResult> UpdateConfirmAsync([FromBody] EdiSonUpdateConfirmPOFFViewModel model)
        {
            var res = await _poFulfillmentService.UpdateAsync(model, CurrentUser);
            return Ok();
        }

        /// <summary>
        /// To create new booking
        /// </summary>
        /// <param name="model">Data</param>
        /// <param name="updateOrganizationPreferences">Whether store organization preferences (HS code, Chinese description)</param>
        /// <returns></returns>
        [HttpPost]
        [AppAuthorize(AppPermissions.PO_Fulfillment_Detail_Edit)]
        public async Task<IActionResult> PostAsync([FromBody] InputPOFulfillmentViewModel model, [FromQuery] bool updateOrganizationPreferences)
        {
            model.ValidateAndThrow();
            model.Audit(null, CurrentUser.Username);
            model.UpdateOrganizationPreferences = updateOrganizationPreferences;
            var result = await _poFulfillmentService.CreateAsync(model, CurrentUser);
            return Ok(result);
        }

        /// <summary>
        /// To update booking
        /// </summary>
        /// <param name="id">Booking Id</param>
        /// <param name="model">Data</param>
        /// <param name="updateOrganizationPreferences">Whether store organization preferences (HS code, Chinese description)</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [AppAuthorize(AppPermissions.PO_Fulfillment_Detail_Edit)]
        public async Task<IActionResult> PutAsync(long id, [FromBody] InputPOFulfillmentViewModel model, [FromQuery] bool updateOrganizationPreferences)
        {
            model.ValidateAndThrow();
            model.Audit(model.Id, CurrentUser.Username);
            model.UpdateOrganizationPreferences = updateOrganizationPreferences;
            var result = await _poFulfillmentService.UpdateAsync(model, CurrentUser);
            return Ok(result);
        }
        #endregion

        [HttpPut("{id}/cancelPOFulfillment")]
        [AppAuthorize(AppPermissions.PO_Fulfillment_Detail_Edit)]
        public async Task<IActionResult> CancelPOFulfillment(long id, CancelPOFulfillmentViewModel cancelModel)
        {
            var poff = await _poFulfillmentService.CancelPOFulfillmentAsync(id, CurrentUser.Username, cancelModel);

            return Ok(poff);
        }

        [HttpPost("{id}/bookingRequests")]
        [AppAuthorize(AppPermissions.PO_Fulfillment_Detail_Edit)]
        public async Task<IActionResult> CreateBookingRequest(long id)
        {
            var poff = await _poFulfillmentService.CreateBookingAsync(id, CurrentUser);
            return Ok(poff);
        }

        [HttpGet("{id}/trialValidateBooking")]
        [AppAuthorize(AppPermissions.PO_Fulfillment_Detail_Edit)]
        public async Task<IActionResult> TrialValidateBooking(long id)
        {
            var result = await _poFulfillmentService.TrialValidateBookingAsync(id, CurrentUser);
            return Ok(result);
        }

        [HttpPut("{id}/amendPOFulfillment")]
        [AppAuthorize(AppPermissions.PO_Fulfillment_Detail_Edit)]
        public async Task<IActionResult> AmendPOFulfillment(long id)
        {
            await _poFulfillmentService.AmendPOFulfillmentAsync(id, CurrentUser.Username);

            return Ok();
        }

        [HttpPut("{id}/planToShip")]
        [AppAuthorize(AppPermissions.PO_Fulfillment_Detail_Edit)]
        public async Task<IActionResult> PlanToShip(long id)
        {
            await _poFulfillmentService.PlanToShipAsync(id, CurrentUser.Username);
            return Ok();
        }

        /// <summary>
        /// Re-load means plan to ship again.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("{id}/re-load")]
        [AppAuthorize(AppPermissions.PO_Fulfillment_Detail_Edit)]
        public async Task<IActionResult> ReloadAsync(long id, [FromBody] InputPOFulfillmentViewModel model)
        {
            model.ValidateAndThrow();
            await _poFulfillmentService.ReloadAsync(id, model, CurrentUser);
            return Ok();
        }

        [HttpPut("{id}/dispatch")]
        [AppAuthorize(AppPermissions.PO_Fulfillment_Detail_Edit)]
        public async Task<IActionResult> Dispatch(long id)
        {
            await _poFulfillmentService.DispatchAsync(id, CurrentUser.Username);

            return Ok();
        }

        [HttpGet("{id}/notes")]
        [AppAuthorize(AppPermissions.PO_Detail)]
        public async Task<IActionResult> GetNotesByPOFulfillment(long id)
        {
            var result = await _noteService.GetPOFulfillmentNotesByIdAsync(id);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{id}/MasterDialogs")]
        [AppAuthorize(AppPermissions.PO_Fulfillment_Detail)]
        public async Task<IActionResult> GetMasterDialogsByPOFulfillment(long id)
        {
            var result = await _globalIdMasterDialogService.GetByPOFulfillmentAsync(id);
            return new JsonResult(result);
        }
    }
}
