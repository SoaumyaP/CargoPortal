using System.Linq;
using System.Threading.Tasks;
using GGroove.CSFE.Application;
using Groove.CSFE.Application.Authorization;
using Groove.CSFE.Application.Carriers.Services;
using Groove.CSFE.Application.Carriers.ViewModels;
using Groove.CSFE.CommonAPI.Filters;
using Groove.CSFE.Core.Data;
using Microsoft.AspNetCore.Mvc;

namespace Groove.CSFE.CommonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize]
    public class CarriersController : ControllerBase
    {
        private readonly ICarrierService _carrierService;

        public CarriersController(ICarrierService carrierService)
        {
            _carrierService = carrierService;
        }

        [HttpGet("search")]
        [AppAuthorize(AppPermissions.Organization_Carrier_List)]
        public async Task<IActionResult> SearchAsync([DataSourceRequest] DataSourceRequest request)
        {
            var viewModels = await _carrierService.ListAsync(request);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("code")]
        [AppAuthorize]
        public async Task<IActionResult> GetByCode(string code)
        {
            var viewModel = await _carrierService.GetByCodeAsync(code);
            return Ok(viewModel);
        }

        [HttpGet]
        [Route("{id}")]
        [AppAuthorize]
        public async Task<IActionResult> GetById(long id)
        {
            var viewModel = await _carrierService.GetByIdAsync(id);
            return Ok(viewModel);
        }

        [HttpGet]
        //TODO:
        //[AppAuthorize(AppPermissions.PO_Detail_Edit, AppPermissions.PO_Fulfillment_Detail_Edit)]
        public IActionResult GetAllCarriers(string code)
        {
            var viewModels = _carrierService.GetAllCarriers(code);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("DropDown")]
        public async Task<IActionResult> GetDropDownAsync(string code)
        {
            var viewModels = await _carrierService.GetAllAsync();
            var result = viewModels?.Select(x => new DropDownModel { Description = x.Id.ToString(), Label = x.CarrierCode });
            return new JsonResult(result);
        }

        [HttpPost]
        [AppAuthorize(AppPermissions.Organization_Carrier_Detail_Add)]
        public async Task<IActionResult> PostAsync(CarrierViewModel model)
        {
            model.Audit(CurrentUser.Username);
            model.MarkAuditFieldStatus();
            var result = await _carrierService.CreateAsync(model);
            return new JsonResult(result);
        }

        [HttpPost]
        [Route("checkDuplicateCarrierCode")]
        [AppAuthorize]
        public async Task<IActionResult> CheckDuplicateCarrierCodeAsync(CarrierViewModel model)
        {
            var result = await _carrierService.CheckDuplicateCarrierCodeAsync(model);
            return new JsonResult(result);
        }

        [HttpPost]
        [Route("checkDuplicateCarrierName")]
        [AppAuthorize]
        public async Task<IActionResult> CheckDuplicateCarrierNameAsync(CarrierViewModel model)
        {
            var result = await _carrierService.CheckDuplicateCarrierNameAsync(model);
            return new JsonResult(result);
        }

        [HttpPost]
        [Route("checkDuplicateCarrierNumber")]
        [AppAuthorize]
        public async Task<IActionResult> CheckDuplicateCarrierNumberAsync(CarrierViewModel model)
        {
            var result = await _carrierService.CheckDuplicateCarrierNumberAsync(model);
            return new JsonResult(result);
        }

        [HttpPut("{id}")]
        [AppAuthorize(AppPermissions.Organization_Carrier_Detail_Edit)]
        public async Task<IActionResult> PutAsync(CarrierViewModel model, long id)
        {
            model.Audit(CurrentUser.Username);
            model.MarkAuditFieldStatus(true);
            var result = await _carrierService.UpdateAsync(model, id);
            return new JsonResult(result);
        }

        [HttpPut("{id}/updateStatus")]
        [AppAuthorize(AppPermissions.Organization_Carrier_Detail_Edit)]
        public async Task<IActionResult> UpdateStatusAsync(long id, CarrierViewModel viewModel)
        {
            var vesselUpdated = await _carrierService.UpdateStatusAsync(id, viewModel.Status, CurrentUser.Username);
            return Ok(vesselUpdated);
        }
    }
}
