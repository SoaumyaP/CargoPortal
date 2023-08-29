using Groove.CSFE.Application.Authorization;
using Groove.CSFE.Application.UserOffices.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Groove.CSFE.CommonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize]
    public class UserOfficesController : ControllerBase
    {
        private readonly IUserOfficeService _userOfficeService;

        public UserOfficesController(IUserOfficeService userOfficeService)
        {
            _userOfficeService = userOfficeService;
        }

        [HttpGet("byLocationName")]
        public async Task<IActionResult> GetByLocationNameAsync([FromQuery] string location, long countryId)
        {
            var res = await _userOfficeService.GetByLocationNameAsync(location, countryId);

            return Ok(res);
        }
    }
}