using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Groove.SP.Application.Shipments.Services.Interfaces;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.CargoDetail.Services.Interfaces;
using Groove.SP.Application.Activity.Services.Interfaces;
using Groove.SP.Application.ShipmentContact.Services.Interfaces;
using Groove.SP.Application.Consignment.Services.Interfaces;
using Groove.SP.Application.Consolidation.Services.Interfaces;
using Groove.SP.Application.Attachment.Services.Interfaces;
using Groove.SP.Application.Container.Services.Interfaces;
using Groove.SP.Application.Itinerary.Services.Interfaces;
using Groove.SP.Application.Translations.Providers.Interfaces;
using Groove.SP.Application.Shipments.ViewModels;
using Groove.SP.Application.ShipmentContact.ViewModels;
using Groove.SP.Core.Models;
using Groove.SP.Application.POFulfillment.Services.Interfaces;
using Groove.SP.Application.Activity.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System;
using Groove.SP.Application.Note.Services.Interfaces;
using Groove.SP.API.Filters;
using Groove.SP.Application.GlobalIdMasterDialog.Services.Interfaces;
using Groove.SP.Application.BillOfLading.ViewModels;
using Groove.SP.Application.MasterBillOfLading.ViewModels;
using Groove.SP.Application.MasterBillOfLading.Services.Interfaces;
using Groove.SP.Core.Data;
using Groove.SP.Infrastructure.CSFE;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities;
using Groove.SP.Application.ViewSetting.Services.Interfaces;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShipmentsController : ControllerBase
    {
        private readonly IShipmentService _shipmentService;
        private readonly IShipmentListService _shipmentListService;
        private readonly IAttachmentService _attachmentService;
        private readonly ICargoDetailService _cargoDetailService;
        private readonly IActivityService _activityService;
        private readonly IShipmentContactService _shipmentContactService;
        private readonly IConsignmentService _consignmentService;
        private readonly IItineraryService _itineraryService;
        private readonly IConsolidationService _consolidationService;
        private readonly IContainerService _containerService;
        private readonly ITranslationProvider _translation;
        private readonly IPOFulfillmentService _poFulfillmentService;
        private readonly INoteService _noteService;
        private readonly IGlobalIdMasterDialogService _globalIdMasterDialogService;
        private readonly IMasterBillOfLadingService _masterBillOfLadingService;
        private readonly ICSFEApiClient _csfeApiClient;
        private readonly IShipmentRepository _shipmentRepository;
        private readonly IContractMasterRepository _contractMasterRepository;
        private readonly IBillOfLadingRepository _billOfLadingRepository;
        private readonly IViewSettingService _viewSettingService;
        private readonly IMasterBillOfLadingRepository _masterBillOfLadingRepository;

        public ShipmentsController(
            IShipmentService shipmentService,
            IAttachmentService attachmentService,
            ICargoDetailService cargoDetailService,
            IActivityService activityService,
            IShipmentContactService shipmentContactService,
            IConsignmentService consignmentService,
            IItineraryService itineraryService,
            IConsolidationService consolidationService,
            IContainerService containerService,
            ITranslationProvider translation,
            IShipmentListService shipmentListService,
            IPOFulfillmentService poFulfillmentService,
            INoteService noteService,
            IGlobalIdMasterDialogService globalIdMasterDialogService,
            IMasterBillOfLadingService masterBillOfLadingService,
            ICSFEApiClient csfeApiClient,
            IContractMasterRepository contractMasterRepository,
            IBillOfLadingRepository billOfLadingRepository,
            IViewSettingService viewSettingService,
            IRepository<MasterBillOfLadingModel> masterBillOfLadingRepository,
            IRepository<ShipmentModel> shipmentRepository)
        {
            _shipmentService = shipmentService;
            _attachmentService = attachmentService;
            _cargoDetailService = cargoDetailService;
            _activityService = activityService;
            _shipmentContactService = shipmentContactService;
            _consignmentService = consignmentService;
            _itineraryService = itineraryService;
            _consolidationService = consolidationService;
            _containerService = containerService;
            _translation = translation;
            _shipmentListService = shipmentListService;
            _poFulfillmentService = poFulfillmentService;
            _noteService = noteService;
            _globalIdMasterDialogService = globalIdMasterDialogService;
            _masterBillOfLadingService = masterBillOfLadingService;
            _csfeApiClient = csfeApiClient;
            _contractMasterRepository = contractMasterRepository;
            _billOfLadingRepository = billOfLadingRepository;
            _viewSettingService = viewSettingService;
            _masterBillOfLadingRepository = (IMasterBillOfLadingRepository)masterBillOfLadingRepository;
            _shipmentRepository = (IShipmentRepository)shipmentRepository;
        }

        #region Charts
        [HttpGet]
        [Route("top5OceanVolumeByOrigin")]
        [AppAuthorize(AppPermissions.Dashboard_MonthlyTop5OceanVolumeByOrigin)]
        [AppDataMemoryCache]
        public async Task<IActionResult> GetTop5OceanVolumeByOrigin(string affiliates, string statisticFilter)
        {
            var viewModels = await _shipmentService.GetTop5OceanVolumeAsync(true, CurrentUser.IsInternal, affiliates, statisticFilter);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("top5OceanVolumeByDestination")]
        [AppAuthorize(AppPermissions.Dashboard_MonthlyTop5OceanVolumeByDestination)]
        [AppDataMemoryCache]
        public async Task<IActionResult> GetTop5OceanVolumeByDestination(string affiliates, string statisticFilter)
        {
            var viewModels = await _shipmentService.GetTop5OceanVolumeAsync(false, CurrentUser.IsInternal, affiliates, statisticFilter);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("top10ConsigneeThisWeek")]
        [AppAuthorize(AppPermissions.Dashboard_Top10ConsigneeThisWeek)]
        [AppDataMemoryCache]
        public async Task<IActionResult> GetTop10ConsigneeThisWeek(string affiliates, string statisticFilter)
        {
            var viewModels = await _shipmentService.GetTop10ThisWeekAsync(OrganizationRole.Consignee, CurrentUser.IsInternal, affiliates, statisticFilter);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("top10ShipperThisWeek")]
        [AppAuthorize(AppPermissions.Dashboard_Top10ShipperThisWeek)]
        [AppDataMemoryCache]
        public async Task<IActionResult> GetTop10ShipperThisWeek(string affiliates, string statisticFilter)
        {
            var viewModels = await _shipmentService.GetTop10ThisWeekAsync(OrganizationRole.Shipper, CurrentUser.IsInternal, affiliates, statisticFilter);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("top10CarrierThisWeek")]
        [AppAuthorize(AppPermissions.Dashboard_Top10CarrierThisWeek)]
        [AppDataMemoryCache]
        public async Task<IActionResult> GetTop10CarrierThisWeek(string affiliates, string statisticFilter)
        {
            var viewModels = await _shipmentService.GetTop10CarrierThisWeekAsync(CurrentUser.IsInternal, affiliates, statisticFilter);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("reporting/weeklyShipments")]
        [AppAuthorize(AppPermissions.Dashboard_ThisWeekShipments)]
        [AppDataMemoryCache]
        public async Task<IActionResult> GetReportingWeeklyShipments(string affiliates, string statisticFilter)
        {
            var result = await _shipmentService.GetReportingWeeklyShipments(CurrentUser.IsInternal, affiliates, statisticFilter);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("reporting/weeklyOceanVolume")]
        [AppAuthorize(AppPermissions.Dashboard_ThisWeekOceanVolume)]
        [AppDataMemoryCache]
        public async Task<IActionResult> GetReportingWeeklyOceanVolume(string affiliates, string statisticFilter)
        {
            var result = await _shipmentService.GetReportingWeeklyOceanVolume(CurrentUser.IsInternal, affiliates, statisticFilter);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("reporting/monthlyOceanVolume")]
        [AppAuthorize(AppPermissions.Dashboard_MonthlyOceanVolumeByMovement, AppPermissions.Dashboard_MonthlyOceanVolumeByServiceType)]
        [AppDataMemoryCache]
        public async Task<IActionResult> GetReportingMonthlyOceanVolume(string groupBy, string affiliates, string statisticFilter)
        {
            var result = await _shipmentService.GetReportingMonthlyOceanVolume(groupBy, CurrentUser.IsInternal, affiliates, statisticFilter);
            return new JsonResult(result);
        }
        #endregion

        [HttpGet]
        [Route("search")]
        [AppAuthorize(AppPermissions.Shipment_List)]
        public async Task<IActionResult> Get([DataSourceRequest] DataSourceRequest request, string affiliates, string statisticKey = "", string statisticFilter = "")
        {
            var viewModels = await _shipmentListService.GetListShipmentAsync(request, CurrentUser.IsInternal, affiliates, "", statisticKey, statisticFilter);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("search/{referenceNo}")]
        [AppAuthorize(AppPermissions.Shipment_Detail)]
        public async Task<IActionResult> GetShipmentsByCustomerReferenceNo([DataSourceRequest] DataSourceRequest request, string referenceNo, string affiliates)
        {
            var viewModels = await _shipmentListService.GetListShipmentAsync(request, CurrentUser.IsInternal, affiliates, referenceNo);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("{id}/checkFullLoadShipment")]
        [AppAuthorize(AppPermissions.Shipment_Detail)]
        public async Task<IActionResult> CheckFullLoadShipmentAsync(long id)
        {
            var result = await _shipmentService.IsFullLoadShipmentAsync(id);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{shipmentNo}")]
        [AppAuthorize(AppPermissions.Shipment_Detail)]
        public async Task<IActionResult> GetDetailsShipment(string shipmentNo, string affiliates)
        {
            var result = await _shipmentService.GetAsync(shipmentNo, CurrentUser.IsInternal, affiliates);

            if (result != null)
            {
                // "Delegation party" displayed only for Admin/Internal/Shipper/Supplier/Delegation/OriginAgent
                var allowedOrganizationRoles = new List<string> { OrganizationRole.Shipper, OrganizationRole.Supplier, OrganizationRole.Delegation, OrganizationRole.OriginAgent };
                var allowedOrganizationIds = result.Contacts?.Where(x => allowedOrganizationRoles.Contains(x.OrganizationRole)).Select(x => x.OrganizationId);

                if (!CurrentUser.IsInternal
                    && allowedOrganizationIds != null
                    && allowedOrganizationIds.Any()
                    && !allowedOrganizationIds.Contains(CurrentUser.OrganizationId)
                    )
                {
                    result.Contacts = result.Contacts.Where(x => !x.OrganizationRole.Equals(OrganizationRole.Delegation)).ToList();
                }

                var checkingRoles = new[] { (long)Role.Shipper, (long)Role.Principal, (long)Role.CruisePrincipal, (long)Role.Agent };

                // Checking Shipper/Principal/Cruise Principal/Agent can ONLY see the value of Contract No if their OrganizationId is matched with ContractHolder 
                if (checkingRoles.Contains(CurrentUser.UserRoleId) && result.ContractMaster != null)
                {
                    if (result.ContractMaster.ContractHolder != CurrentUser.OrganizationId.ToString())
                    {
                        result.ContractMaster.RealContractNo = null;
                    }
                }
            }

            return new JsonResult(result);
        }

        [HttpGet]
        [Route("quicktrack/{shipmentNo}")]
        public async Task<IActionResult> GetQuickTrack(string shipmentNo)
        {
            var result = await _shipmentService.GetQuickTrackAsync(shipmentNo);
            if (result != null) return Ok(result);

            return NotFound(new
            {
                message = await _translation.GetTranslationByKeyAsync("label.quicktrackAPIMessage")
            });
        }

        [HttpGet]
        [Route("{id}/activities")]
        [AppAuthorize(AppPermissions.Shipment_Detail)]
        public async Task<IActionResult> GetActivitiesByShipment(long id)
        {
            var result = await _activityService.GetActivityCrossModuleAsync(EntityType.Shipment, id);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{id}/current-milestone")]
        [AppAuthorize(AppPermissions.Shipment_Detail)]
        public async Task<IActionResult> GetCurrentMilestoneAsync(long id)
        {
            var result = await _shipmentService.GetCurrentMilestoneAsync(id);

            return Ok(result);
        }

        [HttpGet]
        [Route("{id}/attachments")]
        [AppAuthorize(AppPermissions.Shipment_Detail)]
        public async Task<IActionResult> GetAttachmentsByShipment(long id)
        {
            var result = await _attachmentService.GetAttachmentsCrossModuleAsync(EntityType.Shipment, id, CurrentUser.UserRoleId, CurrentUser.OrganizationId);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{id}/cargodetails")]
        [AppAuthorize(AppPermissions.Shipment_Detail)]
        public async Task<IActionResult> GetCargoDetailsByShipment(long id)
        {
            var result = await _cargoDetailService.GetCargoDetailsByShipmentAsync(id);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{id}/contacts")]
        [AppAuthorize(AppPermissions.Shipment_Detail)]
        public async Task<IActionResult> GetContactsByShipment(long id)
        {
            var result = await _shipmentContactService.GetShipmentContactsByShipmentAsync(id);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{id}/consignments")]
        [AppAuthorize(AppPermissions.Shipment_Detail)]
        public async Task<IActionResult> GetConsignmentsByShipment(long id)
        {
            var result = await _consignmentService.GetConsignmentsByShipmentAsync(id);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{id}/itineraries")]
        public async Task<IActionResult> GetItinerariesByShipment(long id, string viewSettingModuleId)
        {
            var result = await _itineraryService.GetItinerariesByShipmentAsync(id);
            if (!string.IsNullOrWhiteSpace(viewSettingModuleId))
            {
                _viewSettingService.SetViewSettingModuleId(viewSettingModuleId, result);
                await _viewSettingService.ApplyViewSettingsAsync(result, CurrentUser.UserRoleId);
            }
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("exceptions")]
        [AppAuthorize(AppPermissions.Shipment_List)]
        public async Task<IActionResult> GetExceptions(string idList)
        {
            var result = await _shipmentService.GetExceptionsAsync(idList);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{id}/consolidations")]
        [AppAuthorize(AppPermissions.Shipment_Detail)]
        public async Task<IActionResult> GetConsolidationsByShipment(long id)
        {
            var result = await _consolidationService.GetConsolidationsByShipmentAsync(id);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{id}/containers")]
        [AppAuthorize(AppPermissions.Shipment_Detail)]
        public async Task<IActionResult> GetContainersByShipment(long id, string affiliates)
        {
            var result = await _containerService.GetContainersByShipmentAsync(id, CurrentUser.IsInternal, affiliates);
            return new JsonResult(result);
        }

        [HttpPost]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
        public async Task<IActionResult> PostAsync([FromBody] ShipmentViewModel model)
        {
            var result = await _shipmentService.CreateAsync(model);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
        public async Task<IActionResult> PutAsync(long id, [FromBody] ShipmentViewModel model)
        {
            var result = await _shipmentService.UpdateAsync(model, id);
            return Ok(result);
        }

        /// <summary>
        /// Call to import Freight Shipment
        /// also support update & delete
        /// </summary>
        /// <param name="model"></param>
        /// <param name="profile"></param>
        /// <returns></returns>
        [HttpPost("import")]
        [AppAuthorize(Scope = "spapi.importfreightshipment")]
        public async Task<IActionResult> ImportFreightShipmentAsync([FromBody] ImportShipmentViewModel model, [FromQuery] string profile)
        {
            var isValid = model.Validate(_csfeApiClient, _shipmentRepository, _contractMasterRepository, out var validationResult);
            if (!isValid)
            {
                await _shipmentService.WriteImportingResultLogAsync(validationResult, model, profile, CurrentUser.Username);
                return BadRequest(validationResult);
            }

            var result = new ImportingShipmentResultViewModel();
            try
            {
                result = await _shipmentService.ImportFreightShipmentAsync(model, CurrentUser.Username);

                if (!result.Success)
                {
                    await _shipmentService.WriteImportingResultLogAsync(result, model, profile, CurrentUser.Username);
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                result.LogErrors(ex.Message);
                await _shipmentService.WriteImportingResultLogAsync(result, model, profile, CurrentUser.Username);

                return StatusCode(500, result);
            }
        }

        /// <summary>
        /// To edit shipment via GUI
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("internal/{id}")]
        [AppAuthorize(AppPermissions.Shipment_Detail_Edit)]
        public async Task<IActionResult> EditShipmentAsync(long id, [FromBody] UpdateShipmentViewModel viewModel)
        {
            var result = await _shipmentService.EditShipmentAsync(id, viewModel, CurrentUser.Username);
            return Ok(result);
        }

        [HttpPut("{id}/cancel")]
        [AppAuthorize(AppPermissions.Shipment_Detail)]

        public async Task<IActionResult> CancelAsync(long id)
        {
            var result = await _shipmentService.CancelShipmentAsync(id, CurrentUser.Username);
            return Ok();
        }

        /// <summary>
        /// Trial validate on cancel shipment. Throw an error if validate failed.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/trialValidateOnCancelShipment")]
        [AppAuthorize(AppPermissions.Shipment_Detail)]
        public async Task<IActionResult> TrialValidateOnCancelShipmentAsync(long id)
        {
            await _shipmentService.TrialValidationOnCancelShipmentAsync(id);
            return Ok();
        }

        /// <summary>
        /// Trial validate on assign HouseBL into shipment. Throw an error if validate failed.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/trialValidateOnAssignHouseBL")]
        [AppAuthorize(AppPermissions.Shipment_Detail)]
        public async Task<IActionResult> TrialValidateOnAssignHouseBLAsync(long id)
        {
            await _shipmentService.TrialValidateOnAssignHouseBLAsync(id);
            return Ok();
        }

        [HttpPut("{id}/confirmItinerary")]
        [AppAuthorize(AppPermissions.Shipment_Detail_Confirm_Itinerary)]
        public async Task<IActionResult> ConfirmItinerary(long id, ShipmentConfirmItineraryViewModel model)
        {
            await _poFulfillmentService.ConfirmItinerariesFromShipmentAsync(id, model, CurrentUser.Username, CurrentUser.CompanyName);
            return Ok();
        }

        [HttpPut("{id}/confirmItineraryUpdates")]
        [AppAuthorize(AppPermissions.Shipment_Detail_Confirm_Itinerary)]
        public async Task<IActionResult> UpdateConfirmItineraryAsync(long id, ShipmentConfirmItineraryViewModel model)
        {
            await _poFulfillmentService.UpdateConfirmItineraryFromShipmentAsync(id, model, CurrentUser.Username);
            return Ok();
        }

        [HttpGet("{id}/defaultCFSClosingDate")]
        public async Task<IActionResult> GetDefaultCFSCloingDateAsync(long id)
        {
            var res = await _shipmentService.GetDefaultCFSClosingDateAsync(id);
            return Ok(res);
        }

        [HttpDelete("{id}")]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
        public async Task<IActionResult> DeleteAsync(long id)
        {
            var result = await _shipmentService.DeleteByKeysAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// To search shipment by shipment number and matched with principal organization id of provided cruise order
        /// </summary>
        /// <param name="shipmentNumber"></param>
        /// <param name="cruiseOrderId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> SearchByShipmentNumber(string shipmentNumber, long cruiseOrderId)
        {
            if (string.IsNullOrEmpty(shipmentNumber) || shipmentNumber.Length < 3 || cruiseOrderId == 0)
            {
                // return empty array;
                return new JsonResult(new List<DropDownListItem>());
            }
            var viewModels = await _shipmentService.SearchShipmentsByShipmentNumberAsync(shipmentNumber, cruiseOrderId);
            return new JsonResult(viewModels);
        }

        #region Contacts

        [HttpPost("{id}/contacts")]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]

        public async Task<IActionResult> PostContactAsync(long id, ShipmentContactViewModel model)
        {
            model.ShipmentId = id;
            var result = await _shipmentContactService.CreateAsync(model);

            return Ok(result);
        }

        [HttpPut("{id}/contacts/{contactId}")]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]

        public async Task<IActionResult> PutContactAsync(long id, long contactId, ShipmentContactViewModel model)
        {
            model.ShipmentId = id;
            var result = await _shipmentContactService.UpdateAsync(model, contactId);
            return Ok(result);
        }

        [HttpDelete("{id}/contacts/{contactId}")]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]

        public async Task<IActionResult> DeleteContactAsync(long contactId)
        {
            var result = await _shipmentContactService.DeleteByKeysAsync(contactId);
            return Ok(result);
        }

        #endregion

        #region Activities
        [HttpPost("{id}/activities")]
        [AppAuthorize(AppPermissions.Shipment_Detail)]
        public async Task<IActionResult> PostActivityAsync(long id, [FromBody] ActivityViewModel model)
        {
            model.Audit(CurrentUser.Username);
            var result = await _activityService.CreateAsync(model);
            return Ok(result);
        }

        [HttpPut("{id}/activities/{activityId}")]
        [AppAuthorize(AppPermissions.Shipment_Detail)]
        public async Task<IActionResult> PutActivityAsync(long id, long activityId, [FromBody] ActivityViewModel model)
        {
            model.Audit(CurrentUser.Username);
            var result = await _activityService.UpdateAsync(model, activityId);
            return Ok();
        }

        [HttpDelete("{id}/activities/{activityId}")]
        [AppAuthorize(AppPermissions.Shipment_Detail)]
        public async Task<IActionResult> DeleteActivityAsync(long activityId)
        {
            await _activityService.DeleteAsync(activityId);
            return Ok();
        }
        #endregion

        #region HouseBL


        [HttpGet("{id}/shipmentLoadDetails")]
        [AppAuthorize()]
        public async Task<IActionResult> GetShipmentLoadDetailsAsync(long id)
        {
            var result = await _shipmentService.GetShipmentLoadDetailsAsync(id);
            return new JsonResult(result);
        }

        [HttpPost("{id}/houseBLs/assign")]
        [AppAuthorize()]
        public async Task<IActionResult> AssignHouseBLToShipmentAsync(long id, BillOfLadingViewModel model)
        {
            await _shipmentService.AssignHouseBLToShipmentAsync(id, model.Id, model.ExecutionAgentId ?? 0, CurrentUser.Username);
            return Ok();
        }

        [HttpPost("{id}/houseBLs")]
        [AppAuthorize()]
        public async Task<IActionResult> CreateHouseBLAsync(long id, BillOfLadingViewModel houseBLViewModel)
        {
            await _shipmentService.CreateAndAssignHouseBLAsync(id, houseBLViewModel, CurrentUser.Username);
            return Ok();
        }
        #endregion

        #region Notes
        [HttpGet("{id}/notes")]
        [AppAuthorize(AppPermissions.Shipment_Detail)]
        public async Task<IActionResult> GetNotesByShipment(long id)
        {
            var result = await _noteService.GetShipmentNotesByIdAsync(id);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{id}/MasterDialogs")]
        [AppAuthorize(AppPermissions.Shipment_Detail)]
        public async Task<IActionResult> GetMasterDialogsByShipment(long id)
        {
            var result = await _globalIdMasterDialogService.GetByShipmentAsync(id);
            return new JsonResult(result);
        }
        #endregion

        #region Master Bill of Ladings

        /// <summary>
        /// To create new master bill of lading from shipment tracking page, via GUI
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("{id}/MasterBillOfLadings")]
        [AppAuthorize(AppPermissions.BillOfLading_MasterBLDetail_Add)]
        public async Task<IActionResult> CreateMasterBLFromShipmentAsync(long id, [FromBody] MasterBillOfLadingViewModel model)
        {
            var result = await _masterBillOfLadingService.CreateFromShipmentAsync(model, id, CurrentUser.Username);
            return new JsonResult(result);
        }

        /// <summary>
        /// To assign master bill of lading to shipment, via GUI
        /// </summary>
        /// <param name="shipmentId"></param>
        /// <param name="masterBOLId"></param>
        /// <returns></returns>
        [HttpPut("{shipmentId}/assignMasterBOL")]
        [AppAuthorize(AppPermissions.BillOfLading_MasterBLDetail_Add)]
        public async Task<IActionResult> AssignMasterBillOfLadingAsync(long shipmentId, long masterBOLId)
        {
            await _shipmentService.AssignMasterBLToShipmentAsync(shipmentId, masterBOLId, CurrentUser.Username);
            return new JsonResult(true);
        }


        /// <summary>
        /// To unlink master bill of lading from provided shipment, via GUI
        /// </summary>
        /// <param name="shipmentId"></param>
        /// <returns></returns>
        [HttpPut("{shipmentId}/unlinkMasterBOL")]
        [AppAuthorize(AppPermissions.BillOfLading_MasterBLDetail_Add)]
        public async Task<IActionResult> UnlinkMasterBillOfLadingAsync(long shipmentId)
        {
            await _shipmentService.UnlinkMasterBillOfLadingAsync(shipmentId, CurrentUser.Username);
            return new JsonResult(true);
        }


        #endregion
    }
}
