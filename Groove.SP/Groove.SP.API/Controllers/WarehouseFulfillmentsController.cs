using Groove.SP.Application.Attachment.Services.Interfaces;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.BuyerApproval.ViewModels;
using Groove.SP.Application.POFulfillmentCargoReceive.Services.Interfaces;
using Groove.SP.Application.WarehouseFulfillment.Services.Interfaces;
using Groove.SP.Application.WarehouseFulfillment.ViewModels;
using Groove.SP.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize]
    public class WarehouseFulfillmentsController : ControllerBase
    {
        private readonly IWarehouseFulfillmentService _warehouseFulfillmentService;
        private readonly IAttachmentService _attachmentService;
        private readonly IPOFulfillmentCargoReceiveService _poFulfillmentCargoReceiveService;
        public WarehouseFulfillmentsController(IWarehouseFulfillmentService warehouseFulfillmentService,
            IAttachmentService attachmentService,
            IPOFulfillmentCargoReceiveService poFulfillmentCargoReceiveService)
        {
            _warehouseFulfillmentService = warehouseFulfillmentService;
            _attachmentService = attachmentService;
            _poFulfillmentCargoReceiveService = poFulfillmentCargoReceiveService;
        }

        [HttpGet]
        [Route("confirm/search")]
        public async Task<IActionResult> SearchWarehouseBookingConfirmAsync(string affiliates)
        {
            var queryParams = HttpContext.Request.Query;
            var filter = new Dictionary<string, string>();

            foreach (var key in queryParams.Keys)
            {
                StringValues @value = new StringValues();
                queryParams.TryGetValue(key, out @value);
                filter.Add(key, @value.ToString());
            }

            var jsonFilter = JsonConvert.SerializeObject(filter);
            var result = await _warehouseFulfillmentService.SearchWarehouseBookingToConfirmAsync(jsonFilter, CurrentUser, affiliates);
            return new JsonResult(result);
        }

        [HttpPut]
        [Route("confirm-by-patch")]
        [AppAuthorize(AppPermissions.PO_Fulfillment_Detail_Edit)]
        public async Task<IActionResult> ConfirmWarehouseBookingsAsync(IEnumerable<InputConfirmWarehouseFulfillmentViewModel> viewModels)
        {
            await _warehouseFulfillmentService.ConfirmWarehouseBookingsAsync( CurrentUser, viewModels);
            return Ok();
        }

        [HttpPut("{id}/approve")]
        [AppAuthorize(AppPermissions.Order_PendingApproval_Detail)]
        public async Task<IActionResult> ApproveAsync(long id, [FromBody] BuyerApprovalViewModel viewModel)
        {
            viewModel.Audit(viewModel.Id, CurrentUser.Username);
            var viewModels = await _warehouseFulfillmentService.ApproveAsync(id, viewModel, CurrentUser.Username);
            return new JsonResult(viewModels);
        }

        [HttpPut("{id}/reject")]
        [AppAuthorize(AppPermissions.Order_PendingApproval_Detail)]
        public async Task<IActionResult> RejectAsync(long id, [FromBody] BuyerApprovalViewModel viewModel)
        {
            viewModel.Audit(viewModel.Id, CurrentUser.Username);
            var viewModels = await _warehouseFulfillmentService.RejectAsync(id, viewModel, CurrentUser.Username);
            return new JsonResult(viewModels);
        }

        [HttpPut("{id}/cancel")]
        [AppAuthorize(AppPermissions.PO_Fulfillment_Detail_Edit)]
        public async Task<IActionResult> CancelAsync(long id, CancelWarehouseFulfillmentViewModel cancelModel)
        {
            var poff = await _warehouseFulfillmentService.CancelAsync(id, CurrentUser.Username, cancelModel);

            return Ok(poff);
        }

        [HttpPut("{id}/cargoReceive")]
        [AppAuthorize(AppPermissions.PO_Fulfillment_Detail_Edit)]
        public async Task<IActionResult> CargoReceiveAsync(long id, [FromBody] List<WarehouseFulfillmentOrderViewModel> viewModels)
        {
            await _warehouseFulfillmentService.CargoReceiveAsync(id, viewModels, CurrentUser);
            return Ok();
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get(long id, string affiliates)
        {
            var viewModel = await _warehouseFulfillmentService.GetAsync(id, CurrentUser.IsInternal, affiliates);
            if (viewModel != null)
            {
                // Load attachments depending on Attachment Type Classification/Permission
                viewModel.Attachments = (await _attachmentService.GetAttachmentsCrossModuleAsync(EntityType.POFullfillment, id, CurrentUser.UserRoleId, CurrentUser.OrganizationId)).ToList();

                return new JsonResult(viewModel);
            }
            return new JsonResult(viewModel);
        }

        [HttpGet]
        [Route("{id}/cargoReceive")]
        [AppAuthorize(AppPermissions.PO_Fulfillment_Detail)]
        public async Task<IActionResult> GetCargoReceiveAsync(long id)
        {
            var viewModel = await _poFulfillmentCargoReceiveService.FirstByPOFulfillmentIdAsync(id);
            return new JsonResult(viewModel);
        }

        /// <summary>
        /// To update warehouse booking
        /// </summary>
        /// <param name="id">Booking Id</param>
        /// <param name="model">Data</param>
        /// <param name="updateOrganizationPreferences">Whether store organization preferences (HS code, Chinese description)</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [AppAuthorize(AppPermissions.PO_Fulfillment_Detail_Edit)]
        public async Task<IActionResult> PutAsync(long id, [FromBody] InputWarehouseFulfillmentViewModel model, [FromQuery] bool updateOrganizationPreferences)
        {
            model.ValidateAndThrow();
            model.Audit(model.Id, CurrentUser.Username);
            model.UpdateOrganizationPreferences = updateOrganizationPreferences;
            var result = await _warehouseFulfillmentService.UpdateAsync(model, CurrentUser);
            return Ok(result);
        }
       
    }
}
