using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.FreightScheduler.Services.Interfaces;
using Groove.SP.Application.FreightScheduler.ViewModels;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Groove.SP.Application.Shipments.Services.Interfaces;
using Groove.SP.Core.Data;
using Groove.SP.API.Filters;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class FreightSchedulersController : ControllerBase
    {
        private readonly IFreightSchedulerService _freightSchedulerService;
        private readonly IShipmentListService _shipmentListService;

        public FreightSchedulersController(IFreightSchedulerService freightSchedulerService, IShipmentListService shipmentListService)
        {
            _freightSchedulerService = freightSchedulerService;
            _shipmentListService = shipmentListService;
        }

        [HttpGet]
        [Route("statistics/vessel-arrival")]
        [AppAuthorize(AppPermissions.Dashboard_EndToEndShipmentStatus)]
        [AppDataMemoryCache]
        public async Task<IActionResult> CountAsync(string affiliates, long? delegatedOrgId, string customerRelationships, string statisticFilter)
        {
            var result = await _freightSchedulerService.CountVesselArrivalAsync(CurrentUser.IsInternal, affiliates, delegatedOrgId, customerRelationships, statisticFilter);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("search")]
        [AppAuthorize(AppPermissions.Shipment_FreightScheduler_List)]
        public async Task<IActionResult> Search([DataSourceRequest] DataSourceRequest request, string affiliates)
        {
            var viewModels = await _freightSchedulerService.ListAsync(request, CurrentUser.IsInternal, affiliates);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("{id}")]
        [AppAuthorize(AppPermissions.Shipment_FreightScheduler_List)]
        public async Task<IActionResult> GetByIdAsync(long id)
        {
            var viewModels = await _freightSchedulerService.GetByIdAsync(id);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("{id}/shipments")]
        public async Task<IActionResult> GetShipmentsAsync([DataSourceRequest] DataSourceRequest request, long id)
        {
            var result = await _shipmentListService.GetListByFreightSchedulerAsync(request, id);
            return new JsonResult(result);
        }

        [HttpPost]
        [AppAuthorize(AppPermissions.Shipment_FreightScheduler_List_Add)]
        public async Task<IActionResult> CreateAsync(FreightSchedulerViewModel model)
        {
            var result = await _freightSchedulerService.CreateAsync(model, CurrentUser.Username);
            return Ok(result);
        }

        [HttpGet]
        [Route("filter")]
        public async Task<IActionResult> Filter()
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
            var result = await _freightSchedulerService.FilterAsync(jsonFilter);
            return new JsonResult(result);
        }

        [HttpPut("{id}")]
        [AppAuthorize(AppPermissions.Shipment_FreightScheduler_List_Edit)]
        public async Task<IActionResult> UpdateAsync(long id, UpdateFreightSchedulerViewModel viewModel)
        {
            var result = await _freightSchedulerService.UpdateAsync(id, viewModel, CurrentUser.Username);
            return Ok(result);
        }

        [HttpPut("edit/{id}")]
        [AppAuthorize(AppPermissions.Shipment_FreightScheduler_List_Edit)]
        public async Task<IActionResult> EditAsync(long id, FreightSchedulerViewModel viewModel)
        {
            var result = await _freightSchedulerService.EditAsync(id, viewModel, CurrentUser.Username);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [AppAuthorize(AppPermissions.Shipment_FreightScheduler_List_Edit)]
        public async Task<IActionResult> DeleteAsync(long id)
        {
            var result = await _freightSchedulerService.DeleteByKeysAsync(id);
            return Ok(result);
        }
    }
}
