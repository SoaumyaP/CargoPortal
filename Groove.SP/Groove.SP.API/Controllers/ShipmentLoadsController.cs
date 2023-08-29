using System.Threading.Tasks;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.ShipmentLoads.Services.Interfaces;
using Groove.SP.Application.ShipmentLoads.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
    public class ShipmentLoadsController : ControllerBase
    {
        private readonly IShipmentLoadService _shipmentLoadService;

        public ShipmentLoadsController(IShipmentLoadService shipmentLoadService)
        {
            _shipmentLoadService = shipmentLoadService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            var result = await _shipmentLoadService.GetAsync(id);
            return new JsonResult(result);
        }

        [HttpPost]
        
        public async Task<IActionResult> PostAsync([FromBody]ShipmentLoadViewModel model)
        {
            var result = await _shipmentLoadService.CreateAsync(model);
            return Ok(result);
        }

        [HttpPut("{id}")]
        
        public async Task<IActionResult> PutAsync(long id, [FromBody]ShipmentLoadViewModel model)
        {
            var result = await _shipmentLoadService.UpdateAsync(model, id);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        
        public async Task<IActionResult> DeleteAsync(long id)
        {
            var result = await _shipmentLoadService.DeleteByKeysAsync(id);
            return Ok(result);
        }
    }
}
