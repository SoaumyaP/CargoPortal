using Groove.SP.Application.Authorization;
using Groove.SP.Application.FreightScheduler.Services.Interfaces;
using Groove.SP.Application.FreightScheduler.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Groove.SP.API.Controllers
{
    [Route("api/schedules")]
    [ApiController]
    [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
    public partial class FreightSchedulesController : ControllerBase
    {
        private readonly IFreightSchedulerService _freightSchedulerService;

        public FreightSchedulesController(IFreightSchedulerService freightSchedulerService)
        {
            _freightSchedulerService = freightSchedulerService;
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateFreightSchedulerApiViewModel viewModel)
        {
            var result = await _freightSchedulerService.UpdateAsync(viewModel, CurrentUser.Username);
            return Ok(result);
        }
    }
}
