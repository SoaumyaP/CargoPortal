using System.Threading.Tasks;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.ShipmentLoadDetails.Services.Interfaces;
using Groove.SP.Application.ShipmentLoadDetails.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
    public class ShipmentLoadDetailsController : ControllerBase
    {
        private readonly IShipmentLoadDetailService _shipmentLoadDetailService;

        public ShipmentLoadDetailsController(IShipmentLoadDetailService shipmentLoadDetailService)
        {
            _shipmentLoadDetailService = shipmentLoadDetailService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            var result = await _shipmentLoadDetailService.GetAsync(id);
            return new JsonResult(result);
        }

        [HttpPost]
        
        public async Task<IActionResult> PostAsync([FromBody]ShipmentLoadDetailViewModel model)
        {
            var result = await _shipmentLoadDetailService.CreateAsync(model);
            return Ok(result);
        }

        [HttpPut("{id}")]
        
        public async Task<IActionResult> PutAsync(long id, [FromBody]ShipmentLoadDetailViewModel model)
        {
            var result = await _shipmentLoadDetailService.UpdateAsync(model, id);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        
        public async Task<IActionResult> DeleteAsync(long id)
        {
            var result = await _shipmentLoadDetailService.DeleteByKeysAsync(id);
            return Ok(result);
        }
    }
}
