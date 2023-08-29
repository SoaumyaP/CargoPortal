using System.Collections.Generic;
using System.Threading.Tasks;
using Groove.SP.API.Filters;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.CargoDetail.Services.Interfaces;
using Groove.SP.Application.CargoDetail.ViewModels;
using Groove.SP.Application.Consignment.Services.Interfaces;
using Groove.SP.Application.Consignment.ViewModels;
using Groove.SP.Application.Consolidation.Services.Interfaces;
using Groove.SP.Application.Consolidation.ViewModels;
using Groove.SP.Application.ShipmentLoadDetails.Services.Interfaces;
using Groove.SP.Application.ShipmentLoadDetails.ViewModels;
using Groove.SP.Application.Shipments.Services.Interfaces;
using Groove.SP.Core.Data;
using Groove.SP.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Groove.SP.API.Controllers
{
    /// <summary>
    /// To be used by CS Portal. Url is /api/consolidations/internal/
    /// </summary>
    [Route("api/consolidations/internal")]
    [ApiController]
    public class InternalConsolidationsController : ControllerBase
    {
        private readonly IConsolidationService _consolidationService;
        private readonly IConsolidationListService _consolidationListService;
        private readonly ICargoDetailService _cargoDetailService;
        private readonly IConsignmentService _consignmentService;
        private readonly IShipmentLoadDetailService _shipmentLoadDetailService;
        private readonly IShipmentService _shipmentService;
        public InternalConsolidationsController(
            ICargoDetailService cargoDetailService,
            IConsolidationService consolidationService,
            IConsignmentService consignmentService,
            IShipmentService shipmentService,
            IShipmentLoadDetailService shipmentLoadDetailService,
            IConsolidationListService consolidationListService
            )
        {
            _consolidationListService = consolidationListService;
            _consolidationService = consolidationService;
            _consignmentService = consignmentService;
            _shipmentService = shipmentService;
            _shipmentLoadDetailService = shipmentLoadDetailService;
            _cargoDetailService = cargoDetailService;

        }

        [HttpGet]
        [Route("search")]
        [AppAuthorize(AppPermissions.Shipment_ConsolidationList)]
        public async Task<IActionResult> Search([DataSourceRequest] DataSourceRequest request, string affiliates)
        {
            var viewModels = await _consolidationListService.ListAsync(request, CurrentUser.IsInternal, affiliates, CurrentUser.OrganizationId);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("{id}")]
        [AppAuthorize(AppPermissions.Shipment_ConsolidationDetail)]
        public async Task<IActionResult> Get(long id, string affiliates)
        {
            var result = await _consolidationService.GetInternalConsolidationAsync(id, CurrentUser.IsInternal, affiliates);
            return new JsonResult(result);
        }

        [HttpPut]
        [Route("{id}")]
        [AppAuthorize(AppPermissions.Shipment_ConsolidationDetail_Edit)]
        public async Task<IActionResult> PutAsync(long id, [FromBody] UpdateConsolidationViewModel model)
        {
            model.ValidateAndThrow();
            var result = await _consolidationService.UpdateConsolidationAsync(model, id, CurrentUser.Username);
            return new JsonResult(result);
        }

        [HttpPut]
        [Route("{id}/shipmentLoadDetails")]
        [AppAuthorize(AppPermissions.Shipment_ConsolidationDetail_Edit)]
        public async Task<IActionResult> PutListAsync([FromBody] IEnumerable<UpdateShipmentLoadDetailViewModel> model, long id)
        {
            var result = await _shipmentLoadDetailService.UpdateRangeByConsolidationAsync(id, model, CurrentUser.Username);
            return Ok(null);
        }

        [HttpPost]
        [AppAuthorize(AppPermissions.Shipment_ConsolidationDetail_Add)]
        public async Task<IActionResult> PostAsync([FromBody] InputConsolidationViewModel model)
        {
            model.ValidateAndThrow();
            model.Audit(null, CurrentUser.Username);
            var result = await _consolidationService.CreateConsolidationAsync(model);
            return Ok(result);
        }

        [HttpGet("{id}/consignments")]
        [AppAuthorize(AppPermissions.Shipment_ConsolidationDetail)]
        public async Task<IActionResult> GetConsignmentList(long id, string affiliates)
        {
            var result = await _consignmentService.GetByConsolidationAsync(id, CurrentUser.IsInternal, affiliates);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{id}/shipmentLoadDetails")]
        [AppAuthorize(AppPermissions.Shipment_ConsolidationDetail)]
        public async Task<IActionResult> ShipmentLoadDetailSearching( [DataSourceRequest] DataSourceRequest request, long id)
        {
            var searchingData = await _shipmentLoadDetailService.GetListByConsolidationAsync(request, id, CurrentUser.IsInternal, CurrentUser.OrganizationId);
            return new JsonResult(searchingData);
        }
        
        /// <summary>
        /// Call to get dropdown shipment number options.
        /// </summary>
        /// <param name="shipmentNumber"></param>
        /// <returns></returns>
        [HttpGet("{id}/shipments/dropdown")]
        public async Task<IActionResult> SearchShipmentNumber(long id, string shipmentNumber)
        {
            if (string.IsNullOrEmpty(shipmentNumber) || shipmentNumber.Length < 3)
            {
                // return empty array;
                return new JsonResult(new List<DropDownListItem>());
            }
            var viewModels = await _shipmentService.SearchShipmentNumberByConsolidationAsync(id, shipmentNumber, CurrentUser.IsInternal, CurrentUser.OrganizationId);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("{id}/cargoDetails")]
        public async Task<IActionResult> GetCargoDetailList(long id)
        {
            var result = await _cargoDetailService.GetUnloadCargoDetailsByConsolidationAsync(id);
            return new JsonResult(result);
        }

        [HttpPost]
        [Route("{id}/cargoDetails")]
        [AppAuthorize(AppPermissions.Shipment_ConsolidationDetail_Edit)]
        public async Task<IActionResult> PostCargoDetailList([FromBody] List<CargoDetailLoadViewModel> model, long id)
        {
            var result = await _consolidationService.LoadCargoDetail(id, model);
            return Ok(result);
        }

        [HttpPost]
        [Route("{id}/consignments/{consignmentId}")]
        [AppAuthorize(AppPermissions.Shipment_ConsolidationDetail_Edit)]
        public async Task<IActionResult> CreateLinkingConsignmentAsync([FromBody] ConsignmentViewModel model, long id, long consignmentId)
        {
            var result = await _consolidationService.CreateLinkingConsignmentAsync(id, consignmentId, model, CurrentUser.Username);
            return Ok(result);
        }

        [HttpPost]
        [Route("{id}/confirm")]
        [AppAuthorize(AppPermissions.Shipment_ConsolidationDetail_Confirm)]
        public async Task<IActionResult> ConfirmConsolidation(long id)
        {
            var result = await _consolidationService.ConfirmConsolidationAsync(id, CurrentUser.IsInternal, CurrentUser.Username);
            return Ok(result);
        }

        [HttpGet]
        [Route("{id}/trialConfirmConsolidation")]
        [AppAuthorize(AppPermissions.Shipment_ConsolidationDetail_Confirm)]
        public async Task<IActionResult> TrialConfirmConsolidationAsync(long id)
        {
            await _consolidationService.ValidateConfirmConsolidationAsync(id);
            return Ok();
        }

        [HttpPost]
        [Route("{id}/unconfirm")]
        [AppAuthorize(AppPermissions.Shipment_ConsolidationDetail_Unconfirm)]
        public async Task<IActionResult> UnconfirmConsolidation(long id)
        {
            var result = await _consolidationService.UnconfirmConsolidationAsync(id, CurrentUser.IsInternal, CurrentUser.Username);
            return Ok(result);
        }

        [HttpDelete]
        [Route("{id}/consignments/{consignmentId}")]
        [AppAuthorize(AppPermissions.Shipment_ConsolidationDetail_Edit)]
        public async Task<IActionResult> DeleteLinkingConsignmentAsync(long id, long consignmentId)
        {
            var result = await _consolidationService.DeleteLinkingConsignmentAsync(id, consignmentId, CurrentUser.Username);
            return Ok(result);
        }

        [HttpDelete]
        [Route("{id}")]
        [AppAuthorize(AppPermissions.Shipment_ConsolidationDetail_Delete)]
        public async Task<IActionResult> DeleteAsync(long id)
        {
            var result = await _consolidationService.DeleteByKeysAsync(id);
            return Ok(result);
        }
    }
}
