using System.Threading.Tasks;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.MasterBillOfLading.Services.Interfaces;
using Groove.SP.Application.MasterBillOfLading.ViewModels;
using Microsoft.AspNetCore.Mvc;


namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
    public class MasterBillOfLadingItinerariesController : ControllerBase
    {
        public readonly IMasterBillOfLadingItineraryService _masterBillOfLadingItineraryService;

        public MasterBillOfLadingItinerariesController(IMasterBillOfLadingItineraryService masterBillOfLadingItineraryService)
        {
            _masterBillOfLadingItineraryService = masterBillOfLadingItineraryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery]long itineraryId, [FromQuery]long masterBillOfLadingId)
        {
            var result = await _masterBillOfLadingItineraryService.GetAsync(itineraryId, masterBillOfLadingId);
            return new JsonResult(result);
        }

        [HttpPost]
        
        public async Task<IActionResult> PostAsync([FromBody]MasterBillOfLadingItineraryViewModel model)
        {
            var result = await _masterBillOfLadingItineraryService.CreateAsync(model);
            return new JsonResult(result);
        }


        [HttpPut]
        
        public async Task<IActionResult> PutAsync([FromQuery]long itineraryId, [FromQuery]long masterBillOfLadingId, [FromBody]MasterBillOfLadingItineraryViewModel model)
        {
            var result = await _masterBillOfLadingItineraryService.UpdateAsync(model, itineraryId, masterBillOfLadingId);
            return new JsonResult(result);
        }

        [HttpDelete]
        
        public async Task<IActionResult> DeleteAsync([FromQuery]long itineraryId, [FromQuery]long masterBillOfLadingId)
        {
            var result = await _masterBillOfLadingItineraryService.DeleteByKeysAsync(itineraryId, masterBillOfLadingId);
            return new JsonResult(result);
        }
    }
}
