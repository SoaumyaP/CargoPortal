using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.BookingValidationLog.Services.Interfaces;
using Groove.SP.Core.Data;
using Groove.SP.API.Filters;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingValidationLogsController : ControllerBase
    {
        private readonly IBookingValidationLogService _bookingValidationLogService;

        public BookingValidationLogsController(
            IBookingValidationLogService bookingValidationLogService)
        {
            _bookingValidationLogService = bookingValidationLogService;
        }
        
        [HttpGet]
        [Route("search")]
        [AppAuthorize(AppPermissions.Organization_Compliance_Detail)]
        public async Task<IActionResult> Search([DataSourceRequest] DataSourceRequest request, long parentId)
        {
            var viewModels = await _bookingValidationLogService.ListAsync(request, parentId);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("{id}")]
        [AppAuthorize(AppPermissions.Organization_Compliance_Detail)]
        public async Task<ActionResult> Get(long id)
        {
            var viewModels = await _bookingValidationLogService.GetAsync(id);
            return new JsonResult(viewModels);
        }
    }
}
