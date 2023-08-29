using System.Threading.Tasks;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.BillOfLading.Services.Interfaces;
using Groove.SP.Application.BillOfLading.ViewModels;
using Microsoft.AspNetCore.Mvc;


namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
    public class BillOfLadingItinerariesController : ControllerBase
    {
        public readonly IBillOfLadingItineraryService _billOfLadingItineraryService;

        public BillOfLadingItinerariesController(IBillOfLadingItineraryService billOfLadingItineraryService)
        {
            _billOfLadingItineraryService = billOfLadingItineraryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery]long itineraryId, [FromQuery]long billOfLadingId)
        {
            var result = await _billOfLadingItineraryService.GetAsync(itineraryId, billOfLadingId);
            return new JsonResult(result);
        }

        [HttpPost]
        
        public async Task<IActionResult> PostAsync([FromBody]BillOfLadingItineraryViewModel model)
        {
            var result = await _billOfLadingItineraryService.CreateAsync(model);
            return new JsonResult(result);
        }


        [HttpPut]
        
        public async Task<IActionResult> PutAsync([FromQuery]long itineraryId, [FromQuery]long billOfLadingId, [FromBody]BillOfLadingItineraryViewModel model)
        {
            var result = await _billOfLadingItineraryService.UpdateAsync(model, itineraryId, billOfLadingId);
            return new JsonResult(result);
        }

        [HttpDelete]
        
        public async Task<IActionResult> DeleteAsync([FromQuery]long itineraryId, [FromQuery]long billOfLadingId)
        {
            var result = await _billOfLadingItineraryService.DeleteByKeysAsync(itineraryId, billOfLadingId);
            return Ok(result);
        }
    }
}
