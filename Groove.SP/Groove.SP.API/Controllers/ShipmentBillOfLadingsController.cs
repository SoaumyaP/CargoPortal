using System.Threading.Tasks;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.ShipmentBillOfLading.Services.Interfaces;
using Groove.SP.Application.ShipmentBillOfLading.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
    public class ShipmentBillOfLadingsController : ControllerBase
    {
        private readonly IShipmentBillOfLadingService _shipmentBillOfLadingService;

        public ShipmentBillOfLadingsController(IShipmentBillOfLadingService shipmentBillOfLadingService)
        {
            _shipmentBillOfLadingService = shipmentBillOfLadingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery]long shipmentId, [FromQuery]long billOfLadingId)
        {
            var result = await _shipmentBillOfLadingService.GetAsync(shipmentId, billOfLadingId);
            return new JsonResult(result);
        }

        [HttpPost]
        
        public async Task<IActionResult> PostAsync([FromBody] ShipmentBillOfLadingViewModel model)
        {
            var result = await _shipmentBillOfLadingService.CreateAsync(model);
            return new JsonResult(result);
        }

        [HttpPut]
        
        public async Task<IActionResult> PutAsync([FromQuery]long shipmentId, [FromQuery]long billOfLadingId, [FromBody] ShipmentBillOfLadingViewModel model)
        {
            var result = await _shipmentBillOfLadingService.UpdateAsync(model, shipmentId, billOfLadingId);
            return new JsonResult(result);
        }

        [HttpDelete]
        
        public async Task<IActionResult> DeleteAsync([FromQuery]long shipmentId, [FromQuery]long billOfLadingId)
        {
            var result = await _shipmentBillOfLadingService.DeleteByKeysAsync(shipmentId, billOfLadingId);
            return new JsonResult(result);
        }
    }
}
