using Groove.SP.API.Models;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.MobileApplication.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Groove.SP.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class MobileApplicationsController: Microsoft.AspNetCore.Mvc.ControllerBase
    {
        private readonly IMobileApplicationService _mobileApplicationSevice;
        public MobileApplicationsController(
            IMobileApplicationService mobileApplicationService)
        {
            _mobileApplicationSevice = mobileApplicationService;
        }

        /// <summary>
        /// To check for mobile application update from provided version. Applied for mobile
        /// </summary>
        /// <param name="version">Current version on mobile</param>
        /// <returns></returns>
        [HttpGet("Updates/{version:required}")]
        [Authorize(Policy = AppConstants.SECURITY_MOBILE_APP_POLICY)]
        public async Task<IActionResult> CheckForUpdateAsync(string version)
        {
           var result = await _mobileApplicationSevice.CheckForUpdateAsync(version);
            return new JsonResult(result);
        }

        /// <summary>
        /// To check for any update for today. Applied for web GUI Dashboard
        /// </summary>
        /// <returns></returns>
        [HttpGet("Updates/Today")]
        [AppAuthorize]
        public async Task<IActionResult> CheckForTodayUpdatesAsync()
        {
            var result = await _mobileApplicationSevice.CheckForTodayUpdatesAsync();
            return new JsonResult(result);
        }

    }
}
