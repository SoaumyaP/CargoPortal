using System.Threading.Tasks;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.BillOfLadingConsignment.Services.Interfaces;
using Groove.SP.Application.BillOfLadingConsignment.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillOfLadingConsignmentsController: ControllerBase
    {
        private readonly IBillOfLadingConsignmentService _billOfLadingConsignmentService;

        public BillOfLadingConsignmentsController(IBillOfLadingConsignmentService billOfLadingService)
        {
            _billOfLadingConsignmentService = billOfLadingService;
        }

        [HttpGet]
        [AppAuthorize]
        public async Task<IActionResult> Get([FromQuery]long consignmentId, [FromQuery]long billOfLadingId)
        {
            var result = await _billOfLadingConsignmentService.GetAsync(billOfLadingId, consignmentId);
            return new JsonResult(result);
        }

        [HttpPost]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
        
        public async Task<IActionResult> Post([FromBody] BillOfLadingConsignmentViewModel model)
        {
            var result = await _billOfLadingConsignmentService.CreateAsync(model);
            return new JsonResult(result);
        }

        [HttpPut]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
        
        public async Task<IActionResult> Put([FromQuery]long consignmentId, [FromQuery]long billOfLadingId, 
                                            [FromBody] BillOfLadingConsignmentViewModel model)
        {
            var result = await _billOfLadingConsignmentService.UpdateAsync(model, billOfLadingId, consignmentId);
            return new JsonResult(result);
        }

        [HttpDelete]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
        
        public async Task<IActionResult> Delete([FromQuery]long consignmentId, [FromQuery]long billOfLadingId)
        {
            var result = await _billOfLadingConsignmentService.DeleteByKeysAsync(billOfLadingId, consignmentId);
            return Ok(result);
        }
    }
}
