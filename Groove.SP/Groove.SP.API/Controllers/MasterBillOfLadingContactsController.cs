
using System.Threading.Tasks;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.MasterBillOfLadingContact.Services.Interfaces;
using Groove.SP.Application.MasterBillOfLadingContact.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
    public class MasterBillOfLadingContactsController : ControllerBase
    {
        private readonly IMasterBillOfLadingContactService _masterBillOfLadingContactService;

        public MasterBillOfLadingContactsController(IMasterBillOfLadingContactService masterBillOfLadingContactService)
        {
            _masterBillOfLadingContactService = masterBillOfLadingContactService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(long id)
        {
            var result = await _masterBillOfLadingContactService.GetAsync(id);
            return new JsonResult(result);
        }

        [HttpPost]
        
        public async Task<IActionResult> PostAsync(long id, [FromBody] MasterBillOfLadingContactViewModel model)
        {
            var result = await _masterBillOfLadingContactService.CreateAsync(model);
            return Ok(result);
        }

        [HttpPut("{id}")]
        
        public async Task<IActionResult> PutAsync(long id, [FromBody] MasterBillOfLadingContactViewModel model)
        {
            var result = await _masterBillOfLadingContactService.UpdateAsync(model, id);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        
        public async Task<IActionResult> DeleteAsync(long id)
        {
            var result = await _masterBillOfLadingContactService.DeleteByKeysAsync(id);
            return Ok(result);
        }
    }
}