using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Groove.SP.API.Filters;
using Groove.SP.Application.Attachment.Services.Interfaces;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.BillOfLading.Services.Interfaces;
using Groove.SP.Application.BillOfLading.ViewModels;
using Groove.SP.Application.BillOfLadingContact.Services.Interfaces;
using Groove.SP.Application.BillOfLadingContact.ViewModels;
using Groove.SP.Application.Container.Services.Interfaces;
using Groove.SP.Application.Itinerary.Services.Interfaces;
using Groove.SP.Application.MasterBillOfLading.Services.Interfaces;
using Groove.SP.Application.MasterBillOfLading.ViewModels;
using Groove.SP.Application.Shipments.Services.Interfaces;
using Groove.SP.Application.Translations.Providers.Interfaces;
using Groove.SP.Core.Data;
using Groove.SP.Core.Models;

using Microsoft.AspNetCore.Mvc;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillOfLadingsController : ControllerBase
    {
        private readonly IBillOfLadingService _billOfLadingService;
        private readonly IShipmentService _shipmentService;
        private readonly IContainerService _containerService;
        private readonly IBillOfLadingContactService _billOfLadingContactService;
        private readonly IItineraryService _itineraryService;
        private readonly IAttachmentService _attachmentService;
        private readonly IMasterBillOfLadingService _masterBillOfLadingService;
        private readonly ITranslationProvider _translation;

        public BillOfLadingsController(
            IBillOfLadingService billOfLadingService,
            IShipmentService shipmentService,
            IContainerService containerService,
            IBillOfLadingContactService billOfLadingContactService,
            IItineraryService itineraryService,
            IAttachmentService attachmentService,
            IMasterBillOfLadingService masterBillOfLadingService,
            ITranslationProvider translation)
        {
            _billOfLadingService = billOfLadingService;
            _shipmentService = shipmentService;
            _containerService = containerService;
            _billOfLadingContactService = billOfLadingContactService;
            _itineraryService = itineraryService;
            _attachmentService = attachmentService;
            _masterBillOfLadingService = masterBillOfLadingService;
            _translation = translation;
        }

        [HttpGet()]
        [Route("{billOfLadingNoOrId}")]
        [AppAuthorize(AppPermissions.BillOfLading_HouseBLDetail)]
        public async Task<IActionResult> GetBOL(string billOfLadingNoOrId, string affiliates)
        {
            var viewModel = await _billOfLadingService.GetBOLAsync(billOfLadingNoOrId, CurrentUser.IsInternal, affiliates);

            if (viewModel != null)
            {
                // "Delegation party" displayed only for Admin/Internal/Shipper/Supplier/Delegation/OriginAgent
                var allowedOrganizationRoles = new List<string> { OrganizationRole.Shipper, OrganizationRole.Supplier, OrganizationRole.Delegation, OrganizationRole.OriginAgent };
                var allowedOrganizationIds = viewModel.Contacts?.Where(x => allowedOrganizationRoles.Contains(x.OrganizationRole)).Select(x => x.OrganizationId);

                if (!CurrentUser.IsInternal
                    && allowedOrganizationIds != null
                    && allowedOrganizationIds.Any()
                    && !allowedOrganizationIds.Contains(CurrentUser.OrganizationId)
                    )
                {
                    viewModel.Contacts = viewModel.Contacts?.Where(x => !x.OrganizationRole.Equals(OrganizationRole.Delegation)).ToList();
                }
            }
            return new JsonResult(viewModel);
        }

        [HttpGet()]
        [Route("quicktrack/{billOfLadingNo}")]
        public async Task<IActionResult> GetQuickTrackAsync(string billOfLadingNo)
        {
            var result = await _billOfLadingService.GetQuickTrackAsync(billOfLadingNo);

            if (result != null)
                return Ok(result);

            return NotFound(new { message = await _translation.GetTranslationByKeyAsync("label.quicktrackAPIMessage") });
        }

        [HttpGet()]
        [Route("{id}/shipments")]
        [AppAuthorize(AppPermissions.BillOfLading_HouseBLDetail)]
        public async Task<IActionResult> GetShipments(long id, string affiliates)
        {
            var viewModel = await _shipmentService.GetShipmentsByBOLAsync(id, CurrentUser.IsInternal, affiliates);
            return new JsonResult(viewModel);
        }

        [HttpGet()]
        [Route("{id}/containers")]
        [AppAuthorize(AppPermissions.BillOfLading_HouseBLDetail)]
        public async Task<IActionResult> GetContainers(long id, string affiliates)
        {
            var viewModel = await _containerService.GetContainersByBOLAsync(id, CurrentUser.IsInternal, affiliates);
            return new JsonResult(viewModel);
        }

        [HttpGet()]
        [Route("{id}/itineraries")]
        [AppAuthorize(AppPermissions.BillOfLading_HouseBLDetail)]
        public async Task<IActionResult> GetItineraries(long id)
        {
            var viewModel = await _itineraryService.GetItinerariesByBOL(id);
            return new JsonResult(viewModel);
        }

        [HttpGet()]
        [Route("{id}/attachments")]
        [AppAuthorize(AppPermissions.BillOfLading_HouseBLDetail)]
        public async Task<IActionResult> GetAttachments(long id)
        {
            var viewModel = await _attachmentService.GetAttachmentsCrossModuleAsync(EntityType.BillOfLading, id, CurrentUser.UserRoleId, CurrentUser.OrganizationId);
            return new JsonResult(viewModel);
        }

        [HttpPost]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]

        public async Task<IActionResult> PostAsync([FromBody] BillOfLadingViewModel model)
        {
            // Not need to audit API as ediSON fulfilled audit information
            var result = await _billOfLadingService.CreateAsync(model);
            return new JsonResult(result);
        }

        [HttpPut("{id}")]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
        public async Task<IActionResult> PutAsync(long id, [FromBody] BillOfLadingViewModel model)
        {
            // Not need to audit API as ediSON fulfilled audit information
            var result = await _billOfLadingService.UpdateAsync(model, id);
            return new JsonResult(result);
        }

        /// <summary>
        /// To Update HouseBL via GUI
        /// </summary>
        /// <param name="id">BillOfLadingId</param>
        /// <param name="model">HouseBL model</param>
        /// <returns></returns>
        [HttpPut("internal/{id}")]
        [AppAuthorize(AppPermissions.BillOfLading_HouseBLDetail_Edit)]
        public async Task<IActionResult> UpdateHouseBLAsync(long id, [FromBody] BillOfLadingViewModel model)
        {
            // Manually audit, ignore CreatedBy/CreateDate
            model.Audit(CurrentUser.Username);
            model.FieldStatus[nameof(BillOfLadingViewModel.CreatedBy)] = FieldDeserializationStatus.WasNotPresent;
            model.FieldStatus[nameof(BillOfLadingViewModel.CreatedDate)] = FieldDeserializationStatus.WasNotPresent;
            model.FieldStatus[nameof(BillOfLadingViewModel.UpdatedBy)] = FieldDeserializationStatus.HasValue;
            model.FieldStatus[nameof(BillOfLadingViewModel.UpdatedDate)] = FieldDeserializationStatus.HasValue;

            var result = await _billOfLadingService.UpdateAsync(model, id);
            return new JsonResult(result);
        }

        [HttpDelete("{id}")]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]

        public async Task<IActionResult> DeleteAsync(long id)
        {
            var result = await _billOfLadingService.DeleteByKeysAsync(id);
            return Ok(result);
        }

        #region HouseBL
        /// <summary>
        /// Check HouseBL already exists before adding new
        /// </summary>
        /// <param name="houseBLNo">BillOfLadingNo inputted</param>
        /// <returns>Return True if HouseBL has already exists</returns>
        [HttpGet("already-exists")]
        [AppAuthorize(AppPermissions.BillOfLading_HouseBLDetail_Add)]
        public async Task<IActionResult> CheckHouseBLAlreadyExistsAsync(string houseBLNo)
        {
            var result = await _billOfLadingService.CheckHouseBLAlreadyExistsAsync(houseBLNo);
            return Ok(result);
        }

        [HttpGet("search")]
        [AppAuthorize(AppPermissions.BillOfLading_ListOfHouseBL)]
        public async Task<IActionResult> SearchAsync([DataSourceRequest] DataSourceRequest request, long roleId, long organizationId, string affiliates)
        {
            var result = await _billOfLadingService.ListAsync(request, CurrentUser.IsInternal, roleId, organizationId, affiliates);
            return Ok(result);
        }

        /// <summary>
        /// This method is used when user search HouseBL No as user is assigning House BL to Shipment via GUI
        /// </summary>
        /// <param name="houseBLNo"></param>
        /// <param name="modeOfTransport"></param>
        /// <param name="executionAgent"></param>
        /// <returns></returns>SearchShipmentAsync
        [HttpGet("houseBLs")]
        public async Task<IActionResult> GetHouseBLAsync(string houseBLNo, string modeOfTransport, long? executionAgent, string affiliates)
        {
            var result = await _billOfLadingService.SearchHouseBLAsync(houseBLNo, modeOfTransport, executionAgent, CurrentUser.IsInternal, affiliates);
            return Ok(result);
        }

        /// <summary>
        /// To get data source for auto-complete as user is searching Shipment to link to House BL
        /// </summary>
        /// <param name="houseBLId">Id of HouseBL</param>
        /// <param name="executionAgentId">executionAgentId of HouseBL</param>
        /// <returns></returns>
        [HttpGet("{houseBLId}/shipment-selection")]
        [AppAuthorize(AppPermissions.BillOfLading_HouseBLDetail)]
        public async Task<IActionResult> SearchShipmentAsync(long houseBLId, string shipmentNo, string modeOfTransport, long executionAgentId, string affiliates)
        {
            var result = await _billOfLadingService.SearchShipmentAsync(houseBLId, shipmentNo, modeOfTransport, executionAgentId, CurrentUser.IsInternal, affiliates);
            return Ok(result);
        }

        /// <summary>
        /// To link a HouseBL to Shipment
        /// </summary>
        /// <param name="shipmentId">shipmentId</param>
        /// <param name="billOfLadingId">HouseBLId</param>
        /// <param name="executionAgentId">ExecutionAgentId of HouseBL</param>
        /// <returns></returns>
        [HttpPut("{houseBLId}/link-to-shipment")]
        [AppAuthorize(AppPermissions.BillOfLading_HouseBLDetail_Edit)]
        public async Task<IActionResult> LinkToShipmentAsync(long shipmentId, long houseBLId, long executionAgentId)
        {
            await _shipmentService.AssignHouseBLToShipmentAsync(shipmentId, houseBLId, executionAgentId, CurrentUser.Username);
            return Ok();
        }

        /// <summary>
        /// To unlink shipment from bill of lading detail page via GUI
        /// </summary>
        /// <param name="houseBLId">BillOfLadingId</param>
        /// <param name="shipmentId">ShipmentId need unlink</param>
        /// <param name="isTheLastLinkedShipment">If the removed shipment is the last one in the list: Remove records in BillOfLadingItineraries - BillOfLadingContacts table</param>
        /// <returns></returns>
        [HttpPut("{houseBLId}/unlink-shipment")]
        [AppAuthorize(AppPermissions.BillOfLading_HouseBLDetail_Edit)]
        public IActionResult UnlinkShipmentAsync(long houseBLId, long shipmentId, int isTheLastLinkedShipment)
        {
            _billOfLadingService.UnlinkShipment(houseBLId, shipmentId, isTheLastLinkedShipment, CurrentUser.Username);
            return Ok();
        }

        #endregion HouseBL

        #region Contacts

        [HttpGet("{id}/contacts")]
        [AppAuthorize(AppPermissions.BillOfLading_HouseBLDetail)]
        public async Task<IActionResult> GetContacts(long id)
        {
            var viewModel = await _billOfLadingContactService.GetBOLContactsByBOLAsync(id);
            return new JsonResult(viewModel);
        }

        [HttpPost("{id}/contacts")]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]

        public async Task<IActionResult> PostContactAsync(long id, [FromBody] BillOfLadingContactViewModel model)
        {
            model.BillOfLadingId = id;
            var result = await _billOfLadingContactService.CreateAsync(model);
            return Ok(result);
        }

        [HttpPut("{id}/contacts/{contactId}")]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]

        public async Task<IActionResult> PutContactAsync(long id, long contactId, [FromBody] BillOfLadingContactViewModel model)
        {
            model.BillOfLadingId = id;
            var result = await _billOfLadingContactService.UpdateAsync(model, contactId);
            return Ok(result);
        }

        [HttpDelete("{id}/contacts/{contactId}")]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]

        public async Task<IActionResult> DeleteContactAsync(long contactId)
        {
            var result = await _billOfLadingContactService.DeleteByKeysAsync(contactId);
            return Ok(result);
        }

        #endregion

        #region Master Bill of Ladings

        /// <summary>
        /// To create new master bill of lading from house bill of lading, via GUI
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("{id}/MasterBillOfLadings")]
        [AppAuthorize(AppPermissions.BillOfLading_MasterBLDetail_Add)]
        public async Task<IActionResult> CreateMasterBLFromHouseBLAsync(long id, [FromBody] MasterBillOfLadingViewModel model)
        {
            var result = await _masterBillOfLadingService.CreateFromHouseBillOfLadingAsync(model, id, CurrentUser.Username);
            return new JsonResult(result);
        }

        /// <summary>
        /// To assign master bill of lading to house bill of lading, via GUI
        /// </summary>
        /// <param name="houseBOLId"></param>
        /// <param name="masterBOLId"></param>
        /// <returns></returns>
        [HttpPut("{houseBOLId}/assignMasterBOL")]
        [AppAuthorize(AppPermissions.BillOfLading_MasterBLDetail_Add)]
        public async Task<IActionResult> AssignMasterBillOfLadingAsync(long houseBOLId, long masterBOLId)
        {
            await _billOfLadingService.AssignMasterBLToHouseBLAsync(houseBOLId, masterBOLId, CurrentUser.Username);
            return new JsonResult(true);
        }

        /// <summary>
        /// To get data source for house bill of lading auto-complete as user is adding house bl to master bl via GUI
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="affiliates"></param>
        /// <returns></returns>
        [HttpGet("searchByNumber")]
        [AppAuthorize(AppPermissions.BillOfLading_MasterBLDetail_Add)]
        public async Task<IActionResult> GetBillOfLadingListBySearchingNumberAsync(string searchTerm, string affiliates)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                throw new System.ApplicationException("Filter set is not correct.");
            }
            var viewModels = await _billOfLadingService.GetBillOfLadingListBySearchingNumberAsync(searchTerm, CurrentUser.IsInternal, affiliates);
            return new JsonResult(viewModels);
        }

        #endregion
    }
}
