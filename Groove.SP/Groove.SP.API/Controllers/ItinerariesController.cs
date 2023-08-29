using Groove.SP.Application.Authorization;
using Groove.SP.Application.Itinerary.Services.Interfaces;
using Groove.SP.Application.Itinerary.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
    public class ItinerariesController : ControllerBase
    {
        private readonly IItineraryService _itineraryService;

        public ItinerariesController(IItineraryService itineraryService)
        {
            _itineraryService = itineraryService;
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] CreateItineraryViewModel model)
        {
            model.IsImportFromApi = true;
            var result = await _itineraryService.CreateAsync(model);
            return new JsonResult(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(long id, [FromBody] UpdateItineraryViewModel model)
        {
            var result = await _itineraryService.UpdateAsync(model, id);
            return new JsonResult(result);
        }

        [HttpDelete("{id}")]
        
        public async Task<IActionResult> DeleteAsync(long id)
        {
            var result = await _itineraryService.DeleteByKeysAsync(id);
            return new JsonResult(result);
        }
    }
}
