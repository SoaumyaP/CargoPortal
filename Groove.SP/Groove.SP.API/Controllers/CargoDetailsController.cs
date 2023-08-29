using System.Threading.Tasks;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.CargoDetail.Services.Interfaces;
using Groove.SP.Application.CargoDetail.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
    public class CargoDetailsController : ControllerBase
    {
        private readonly ICargoDetailService _cargoDetailService;

        public CargoDetailsController(ICargoDetailService cargoDetailService)
        {
            _cargoDetailService = cargoDetailService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(long id)
        {
            var result = await _cargoDetailService.GetAsync(id);
            return new JsonResult(result);
        }

        [HttpPost]
        
        public async Task<IActionResult> PostAsync([FromBody]CargoDetailViewModel model)
        {
            var result = await _cargoDetailService.CreateAsync(model);
            return Ok(result);
        }

        [HttpPut("{id}")]
        
        public async Task<IActionResult> PutAsync(long id, [FromBody]CargoDetailViewModel model)
        {
            var result = await _cargoDetailService.UpdateAsync(model, id);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        
        public async Task<IActionResult> DeleteAsync(long id)
        {
            var result = await _cargoDetailService.DeleteByKeysAsync(id);
            return Ok(result);
        }
    }
}
