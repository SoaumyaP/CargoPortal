using Groove.SP.Application.Attachment.Services.Interfaces;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.CargoDetail.Services.Interfaces;
using Groove.SP.Application.Consignment.Services.Interfaces;
using Groove.SP.Application.Consignment.ViewModels;
using Groove.SP.Application.Container.Services.Interfaces;
using Groove.SP.Application.ShipmentContact.Services.Interfaces;
using Groove.SP.Application.Itinerary.Services.Interfaces;
using Groove.SP.Application.Itinerary.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.Activity.Services.Interfaces;
using Groove.SP.Application.Consolidation.Services.Interfaces;
using System.Collections.Generic;
using Groove.SP.Core.Models;
using System.Linq;
using Groove.SP.Core.Data;
using Groove.SP.API.Filters;
using Groove.SP.Application.PurchaseOrders.Services.Interfaces;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsignmentsController : ControllerBase
    {
        private readonly IConsignmentService _consignmentService;
        private readonly IAttachmentService _attachmentService;
        private readonly IConsolidationService _consolidationService;
        private readonly IContainerService _containerService;
        private readonly ICargoDetailService _cargoDetailService;
        private readonly IShipmentContactService _shipmentContactService;
        private readonly IItineraryService _itineraryService;
        private readonly IActivityService _activityService;
        public readonly IConsignmentItineraryService _consignmentItineraryService;
        public readonly IConsignmentListService _consignmentListService;

        public ConsignmentsController(IConsignmentService consignmentService,
            IAttachmentService attachmentService,
            IConsolidationService consolidationService,
            IContainerService containerService,
            ICargoDetailService cargoDetailService,
            IShipmentContactService shipmentContactService,
             IItineraryService itineraryService,
            IConsignmentItineraryService consignmentItineraryService,
            IActivityService activityService,
            IConsignmentListService consignmentListService)
        {
            _consignmentService = consignmentService;
            _attachmentService = attachmentService;
            _consolidationService = consolidationService;
            _containerService = containerService;
            _cargoDetailService = cargoDetailService;
            _shipmentContactService = shipmentContactService;
            _itineraryService = itineraryService;
            _consignmentItineraryService = consignmentItineraryService;
            _activityService = activityService;
            _consignmentListService = consignmentListService;
        }

        [HttpGet]
        [Route("search")]
        [AppAuthorize(AppPermissions.Shipment_Consignment_List)]
        public async Task<IActionResult> Get([DataSourceRequest] DataSourceRequest request, string affiliates)
        {
            var viewModels = await _consignmentListService.GetListConsignmentAsync(request, CurrentUser.IsInternal, affiliates);
            return new JsonResult(viewModels);
        }

        [HttpGet("{id}")]
        [AppAuthorize(AppPermissions.Shipment_Consignment_Detail)]
        public async Task<IActionResult> GetAsync(long id, string affiliates)
        {
            var result = await _consignmentService.GetAsync(id, CurrentUser.IsInternal, affiliates);
            return new JsonResult(result);
        }

        [HttpPost]
        
        [AppAuthorize(AppPermissions.Shipment_Consignment_Detail_Add)]
        public async Task<IActionResult> PostAsync([FromBody]ConsignmentViewModel model)
        {
            model.Audit(CurrentUser.Username);
            var result = await _consignmentService.CreateAsync(model);
            return Ok(result);
        }

        [HttpPut("{id}/trash")]
        [AppAuthorize(AppPermissions.Shipment_Consignment_Detail_Edit)]
        public async Task<IActionResult> MoveToStrashAsync(long id)
        {
            await _consignmentService.MoveToTrashAsync(id, CurrentUser.Username);
            return Ok();
        }


        [HttpPut("{id}")]
        
        [AppAuthorize(AppPermissions.Shipment_Consignment_Detail_Edit)]
        public async Task<IActionResult> PutAsync(long id, [FromBody]ConsignmentViewModel model)
        {
            model.Audit(CurrentUser.Username);
            var result = await _consignmentService.UpdateAsync(model, id);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        
        [AppAuthorize(AppPermissions.Shipment_Consignment_Detail_Edit)]
        public async Task<IActionResult> DeleteAsync(long id)
        {
            var result = await _consignmentService.DeleteByKeysAsync(id);
            return Ok(result);
        }

        [HttpGet]
        [Route("{id}/attachments")]
        [AppAuthorize(AppPermissions.Shipment_Consignment_Detail)]
        public async Task<IActionResult> GetAttachments(long id)
        {
            var result = await _attachmentService.GetAttachmentsAsync(EntityType.Consignment, id, CurrentUser.UserRoleId);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{consignmentId}/consolidations")]
        [AppAuthorize(AppPermissions.Shipment_Consignment_Detail)]
        public async Task<IActionResult> GetConsolidations(long consignmentId)
        {
            var result = await _consolidationService.GetConsolidationsByConsignmentAsync(consignmentId);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{shipmentId}/containers")]
        [AppAuthorize(AppPermissions.Shipment_Consignment_Detail)]
        public async Task<IActionResult> GetContainers(long shipmentId, string affiliates)
        {
            var result = await _containerService.GetContainersByShipmentAsync(shipmentId, CurrentUser.IsInternal, affiliates);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{shipmentId}/cargodetails")]
        [AppAuthorize(AppPermissions.Shipment_Consignment_Detail)]
        public async Task<IActionResult> GetCargoDetails(long shipmentId)
        {
            var result = await _cargoDetailService.GetCargoDetailsByShipmentAsync(shipmentId);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{shipmentId}/contacts")]
        [AppAuthorize(AppPermissions.Shipment_Consignment_Detail)]
        public async Task<IActionResult> GetContacts(long shipmentId)
        {
            var result = await _shipmentContactService.GetShipmentContactsByShipmentAsync(shipmentId);

            if (result != null)
            {
                // "Delegation party" displayed only for Admin/Internal/Shipper/Supplier/Delegation/OriginAgent
                var allowedOrganizationRoles = new List<string> { OrganizationRole.Shipper, OrganizationRole.Supplier, OrganizationRole.Delegation, OrganizationRole.OriginAgent };
                var allowedOrganizationIds = result.ToList().Where(x => allowedOrganizationRoles.Contains(x.OrganizationRole)).Select(x => x.OrganizationId);

                if (!CurrentUser.IsInternal
                    && allowedOrganizationIds != null
                    && allowedOrganizationIds.Any()
                    && !allowedOrganizationIds.Contains(CurrentUser.OrganizationId)
                    )
                {
                    result = result.Where(x => !x.OrganizationRole.Equals(OrganizationRole.Delegation));
                }
            }
            return new JsonResult(result);
        }

        /// <summary>
        /// To get data source for drop-down as adding new Consignment into Consolidation via GUI
        /// </summary>
        /// <param name="shipmentId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{shipmentId}/dropdown")]
        public async Task<IActionResult> GetConsignmentDropdownByShipmentId(long shipmentId)
        {
            var result = await _consignmentService.GetDropdownByShipmentAsync(shipmentId);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{id}/origincfs")]
        public async Task<IActionResult> GetShipmentOriginCFS(long id)
        {
            var result = await _consignmentService.GetDropdownOriginCFSAsync(id);
            return new JsonResult(result);
        }

        #region Itinerary
        [HttpGet]
        [Route("{id}/itineraries")]
        [AppAuthorize(AppPermissions.Shipment_Consignment_Detail)]
        public async Task<IActionResult> GetItinerariesByConsignment(long id)
        {
            var result = await _itineraryService.GetItinerariesByConsignmentAsync(id, CurrentUser.Username, CurrentUser.IsInternal, CurrentUser.UserRoleId);
            return new JsonResult(result);
        }

        /// <summary>
        /// Called from application GUI to add Itinerary on Shipment or Consignment page
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("{id}/itineraries")]
        [AppAuthorize(AppPermissions.Shipment_Consignment_Detail_Add)]
        public async Task<IActionResult> PostItineraryAsync(long id, string affiliates, [FromBody] CreateItineraryViewModel model)
        {
            model.Audit(CurrentUser.Username);
            var result = await _itineraryService.CreateAsync(model, id, CurrentUser, affiliates);
            return Ok(result);
        }

        /// <summary>
        /// Called from application GUI to update Itinerary on Shipment or Consignment page
        /// </summary>
        /// <param name="id"></param>
        /// <param name="itineraryId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{id}/itineraries/{itineraryId}")]
        [AppAuthorize(AppPermissions.Shipment_Consignment_Detail_Edit)]
        public async Task<IActionResult> PutItineraryAsync(long id, long itineraryId, string affiliates, [FromBody] UpdateItineraryViewModel model)
        {
            model.Audit(CurrentUser.Username);
            var result = await _itineraryService.UpdateAsync(model, id, CurrentUser, affiliates);
            return Ok(result);
        }

        /// <summary>
        /// Called from application GUI to remove Itinerary on Shipment or Consignment page
        /// </summary>
        /// <param name="id"></param>
        /// <param name="itineraryId"></param>
        /// <param name="affiliates"></param>
        /// <returns></returns>
        [HttpDelete("{id}/itineraries/{itineraryId}")]
        [AppAuthorize(AppPermissions.Shipment_Consignment_Detail_Edit)]
        public async Task<IActionResult> DeleteItineraryAsync(long id, long itineraryId, string affiliates)
        {
            await _itineraryService.DeleteAsync(itineraryId, id, CurrentUser, affiliates);
            return Ok();
        }
        #endregion

        [HttpGet]
        [Route("{id}/activities")]
        [AppAuthorize(AppPermissions.Shipment_Consignment_Detail)]
        public async Task<IActionResult> GetActivitiesByConsignment(long id)
        {
            var result = await _activityService.GetActivities(EntityType.Consignment, id);
            return new JsonResult(result);
        }

        [HttpPost("{id}/activities")]
        [AppAuthorize(AppPermissions.Shipment_Consignment_Detail_Add)]
        public async Task<IActionResult> PostActivityAsync(long id, [FromBody] ActivityViewModel model)
        {
            model.Audit(CurrentUser.Username);
            model.ConsignmentId = model.ConsignmentId;
            model.ShipmentId = model.ShipmentId;
            var result = await _activityService.CreateAsync(model);
            return Ok(result);
        }

        [HttpPut("{id}/activities/{activityId}")]
        [AppAuthorize(AppPermissions.Shipment_Consignment_Detail_Edit)]
        public async Task<IActionResult> PutActivityAsync(long id, long activityId, [FromBody] ActivityViewModel model)
        {
            model.Audit(CurrentUser.Username);
            model.ConsignmentId = model.ConsignmentId;
            model.ShipmentId = model.ShipmentId;
            var result = await _activityService.UpdateAsync(model, activityId);
            return Ok();
        }

        [HttpDelete("{id}/activities/{activityId}")]
        [AppAuthorize(AppPermissions.Shipment_Consignment_Detail_Edit)]
        public async Task<IActionResult> DeleteActivityAsync(long activityId)
        {
            var result = await _activityService.DeleteByKeysAsync(activityId);
            return Ok();
        }
    }
}
