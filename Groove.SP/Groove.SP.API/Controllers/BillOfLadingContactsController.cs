using System.Threading.Tasks;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.BillOfLadingContact.Services.Interfaces;
using Groove.SP.Application.BillOfLadingContact.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
    public class BillOfLadingContactsController : ControllerBase
    {
        private readonly IBillOfLadingContactService _billOfLadingContactService;

        public BillOfLadingContactsController(IBillOfLadingContactService billOfLadingContactService)
        {
            _billOfLadingContactService = billOfLadingContactService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(long id)
        {
            var result = await _billOfLadingContactService.GetAsync(id);
            return new JsonResult(result);
        }

        [HttpPost]
        
        public async Task<IActionResult> PostAsync(long id, [FromBody] BillOfLadingContactViewModel model)
        {
            var result = await _billOfLadingContactService.CreateAsync(model);
            return Ok(result);
        }

        [HttpPut("{id}")]
        
        public async Task<IActionResult> PutAsync(long id, [FromBody] BillOfLadingContactViewModel model)
        {
            var result = await _billOfLadingContactService.UpdateAsync(model, id);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        
        public async Task<IActionResult> DeleteAsync(long id)
        {
            var result = await _billOfLadingContactService.DeleteByKeysAsync(id);
            return Ok(result);
        }
    }
}