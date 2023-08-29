using Groove.CSFE.Application.Authorization;
using Groove.CSFE.Application.Warehouses.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Groove.CSFE.CommonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize]
    public class WarehousesController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;
        public WarehousesController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        [HttpGet]
        [Route("dropdown")]
        public async Task<IActionResult> GetDropdownAsync([FromQuery] string searchTerm)
        {
            var viewModels = await _warehouseService.GetDropdownAsync(searchTerm);
            return new JsonResult(viewModels);
        }
    }
}