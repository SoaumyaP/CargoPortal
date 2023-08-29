using System.Threading.Tasks;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.BillOfLadingShipmentLoad.Services.Interfaces;
using Groove.SP.Application.BillOfLadingShipmentLoad.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
    public class BillOfLadingShipmentLoadsController : ControllerBase
    {
        private readonly IBillOfLadingShipmentLoadService _billOfLadingShipmentLoadService;

        public BillOfLadingShipmentLoadsController(IBillOfLadingShipmentLoadService billOfLadingShipmentLoadService)
        {
            _billOfLadingShipmentLoadService = billOfLadingShipmentLoadService;
        }

        [HttpGet]     
        public async Task<IActionResult> GetAsync([FromQuery]long billOfLadingId, [FromQuery]long shipmentLoadId)
        {
            var result = await _billOfLadingShipmentLoadService.GetAsync(billOfLadingId, shipmentLoadId);
            return new JsonResult(result);
        }

        [HttpPost]
        
        public async Task<IActionResult> PostAsync([FromBody] BillOfLadingShipmentLoadViewModel model)
        {
            var result = await _billOfLadingShipmentLoadService.CreateAsync(model);
            return new JsonResult(result);
        }

        [HttpPut]
        
        public async Task<IActionResult> PutAsync([FromQuery]long billOfLadingId, [FromQuery]long shipmentLoadId, [FromBody] BillOfLadingShipmentLoadViewModel model)
        {
            var result = await _billOfLadingShipmentLoadService.UpdateAsync(model, billOfLadingId, shipmentLoadId);
            return new JsonResult(result);
        }

        [HttpDelete]
        
        public async Task<IActionResult> DeleteAsync([FromQuery]long billOfLadingId, [FromQuery]long shipmentLoadId)
        {
            var result = await _billOfLadingShipmentLoadService.DeleteByKeysAsync(billOfLadingId, shipmentLoadId);
            return Ok(result);
        }
    }
}
