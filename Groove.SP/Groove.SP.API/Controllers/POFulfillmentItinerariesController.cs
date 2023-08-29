using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.POFulfillment.Services.Interfaces;
using Groove.SP.Application.POFulfillment.ViewModels;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize]
    public class POFulfillmentItinerariesController : ControllerBase
    {
        private readonly IEdiSonConfirmService _ediSonConfirmService;

        public POFulfillmentItinerariesController(IEdiSonConfirmService ediSonConfirmService)
        {
            _ediSonConfirmService = ediSonConfirmService;
        }

        [HttpPost]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
        
        public async Task<IActionResult> PostAsync([FromBody] EdiSonConfirmPOFFViewModel model)
        {
            await _ediSonConfirmService.ConfirmPOFFAsync(model);
            return Ok();
        }
    }
}
