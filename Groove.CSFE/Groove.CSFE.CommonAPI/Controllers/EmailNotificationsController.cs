using Groove.CSFE.Application.EmailNotification.Services.Interfaces;
using Groove.CSFE.Application.EmailNotification.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Groove.CSFE.CommonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailNotificationsController : ControllerBase
    {
        private readonly IEmailNotificationService _emailNotificationService;

        public EmailNotificationsController(IEmailNotificationService emailNotificationService)
        {
            _emailNotificationService = emailNotificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync(long organizationId, long customerId, long locationId)
        {
            var result = await _emailNotificationService.GetAsync(organizationId, customerId, locationId);
            return new JsonResult(result);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetListAsync(long organizationId, long customerId, long locationId)
        {
            var result = await _emailNotificationService.GetListAsync(organizationId, customerId, locationId);
            return new JsonResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] EmailNotificationViewModel model)
        {
            model.Audit(CurrentUser.Username);
            var result = await _emailNotificationService.CreateAsync(model);
            return new JsonResult(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync([FromBody] EmailNotificationViewModel model, long id)
        {
            model.Audit(CurrentUser.Username);
            var result = await _emailNotificationService.UpdateAsync(model, id);
            return new JsonResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(long id)
        {
            var result = await _emailNotificationService.DeleteAsync(id);
            return Ok(result);
        }
    }
}