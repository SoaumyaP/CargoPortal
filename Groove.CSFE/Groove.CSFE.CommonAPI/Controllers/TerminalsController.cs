using Groove.CSFE.Application.Authorization;
using Groove.CSFE.Application.Terminals.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Groove.CSFE.CommonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize]
    public class TerminalsController : ControllerBase
    {
        private readonly ITerminalService _terminalService;
        public TerminalsController(ITerminalService terminalService)
        {
            _terminalService = terminalService;
        }

        [HttpGet]
        [Route("dropdown")]
        public async Task<IActionResult> GetDropdownAsync([FromQuery] string searchTerm)
        {
            var viewModels = await _terminalService.GetDropdownAsync(searchTerm);
            return new JsonResult(viewModels);
        }
    }
}