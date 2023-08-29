using System.Threading.Tasks;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.Container.Services.Interfaces;
using Groove.SP.Application.Container.ViewModels;
using Microsoft.AspNetCore.Mvc;


namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
    public class ContainerItinerariesController : ControllerBase
    {
        public readonly IContainerItineraryService _containerItineraryService;

        public ContainerItinerariesController(IContainerItineraryService containerItineraryService)
        {
            _containerItineraryService = containerItineraryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery]long itineraryId, [FromQuery]long containerId)
        {
            var result = await _containerItineraryService.GetAsync(itineraryId, containerId);
            return new JsonResult(result);
        }

        [HttpPost]
        
        public async Task<IActionResult> PostAsync([FromBody]ContainerItineraryViewModel model)
        {
            var result = await _containerItineraryService.CreateAsync(model);
            return new JsonResult(result);
        }


        [HttpPut]
        
        public async Task<IActionResult> PutAsync([FromQuery]long itineraryId, [FromQuery]long containerId, [FromBody]ContainerItineraryViewModel model)
        {
            var result = await _containerItineraryService.UpdateAsync(model, itineraryId, containerId);
            return new JsonResult(result);
        }

        [HttpDelete]
        
        public async Task<IActionResult> DeleteAsync([FromQuery]long itineraryId, [FromQuery]long containerId)
        {
            var result = await _containerItineraryService.DeleteByKeysAsync(itineraryId, containerId);
            return new JsonResult(result);
        }
    }
}
