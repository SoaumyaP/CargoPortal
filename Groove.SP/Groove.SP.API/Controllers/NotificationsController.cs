using Groove.SP.Application.Authorization;
using Groove.SP.Application.Notification.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("byUser")]
        [AppAuthorize]
        public async Task<IActionResult> GetAsync(int skip, int take)
        {
            var viewModels = await _notificationService.GetByUserNameAsync(CurrentUser.Username, skip, take);

            return new JsonResult(viewModels);
        }

        [HttpGet("unreadTotal")]
        [AppAuthorize]
        public async Task<IActionResult> GetUnreadTotalAsync()
        {
            var result = await _notificationService.GetUnreadTotalAsync(CurrentUser.Username);

            return new JsonResult(result);
        }

        [HttpPut("{id}/read")]
        [AppAuthorize]
        public async Task<IActionResult> ReadAsync(long id)
        {
            await _notificationService.ReadAsync(id, CurrentUser.Username);

            return Ok();
        }

        [HttpPut("readAll")]
        [AppAuthorize]
        public async Task<IActionResult> ReadAllAsync()
        {
            await _notificationService.ReadAllAsync(CurrentUser.Username);

            return Ok();
        }
    }
}
