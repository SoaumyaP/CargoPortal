using Groove.CSFE.Application.Authorization;
using Groove.CSFE.Application.WarehouseLocations.Services.Interfaces;
using Groove.CSFE.Application.WarehouseLocations.ViewModels;
using Groove.CSFE.CommonAPI.Filters;
using Groove.CSFE.Core.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Groove.CSFE.CommonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize]
    public class WarehouseLocationsController : ControllerBase
    {
        private readonly IWarehouseLocationService _warehouseLocationService;
        private readonly IWarehouseLocationListService _warehouseLocationListService;

        public WarehouseLocationsController(IWarehouseLocationService warehouseLocationService,
            IWarehouseLocationListService warehouseLocationListService)
        {
            _warehouseLocationService = warehouseLocationService;
            _warehouseLocationListService = warehouseLocationListService;
        }

        [HttpGet]
        [Route("dropdown")]
        public async Task<IActionResult> GetWarehouseLocationDropdownAsync([FromQuery] string searchTerm)
        {
            var viewModels = await _warehouseLocationService.GetDropdownAsync(searchTerm);
            return new JsonResult(viewModels);
        }

        [HttpGet("search")]
        [AppAuthorize(AppPermissions.Organization_WarehouseLocation_List)]
        public async Task<IActionResult> SearchAsync([DataSourceRequest] DataSourceRequest request)
        {
            var viewModels = await _warehouseLocationListService.GetListWarehouseLocationAsync(request);
            return new JsonResult(viewModels);
        }

        [HttpPost()]
        [AppAuthorize(AppPermissions.Organization_WarehouseLocation_Detail_Add)]
        public async Task<IActionResult> CreateWarehouseLocationAsync(WarehouseLocationViewModel viewModel)
        {
            var result = await _warehouseLocationService.CreateWarehouseLocationAsync(viewModel, CurrentUser.Username);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [AppAuthorize(AppPermissions.Organization_WarehouseLocation_Detail_Edit)]
        public async Task<IActionResult> UpdateWarehouseLocationAsync(WarehouseLocationViewModel viewModel)
        {
            viewModel.Audit(CurrentUser.Username);
            var result = await _warehouseLocationService.UpdateWarehouseLocationAsync(viewModel, CurrentUser.Username);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AppAuthorize(AppPermissions.Organization_WarehouseLocation_Detail)]
        public async Task<IActionResult> GetAsync(long id)
        {
            var viewModels = await _warehouseLocationService.GetByKeysAsync(id);
            return Ok(viewModels);
        }

        [HttpGet("{id}/customers")]
        [AppAuthorize(AppPermissions.Organization_WarehouseLocation_Detail)]
        public async Task<IActionResult> GetCustomersAsync(long id)
        {
            var viewModels = await _warehouseLocationService.GetCustomersByWarehouseLocationIdAsync(id);
            return Ok(viewModels);
        }
    }
}
