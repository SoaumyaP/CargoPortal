using Groove.SP.API.Filters;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.IntegrationLog.Services.Interfaces;
using Groove.SP.Application.IntegrationLog.ViewModels;
using Groove.SP.Core.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IntegrationLogsController : ControllerBase
    {
        private readonly IIntegrationLogService _integrationLogService;
        public IntegrationLogsController(IIntegrationLogService integrationLogService)
        {
            _integrationLogService = integrationLogService;
        }
        
        [HttpGet]
        [Route("search")]
        [AppAuthorize(AppPermissions.IntegrationLog_List)]
        public async Task<ActionResult> Search([DataSourceRequest] DataSourceRequest request)
        {
            var viewModels = await _integrationLogService.GetListAsync(request);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("{id}")]
        [AppAuthorize(AppPermissions.IntegrationLog_List)]
        public async Task<ActionResult> Get(long id)
        {
            var viewModels = await _integrationLogService.GetAsync(id);
            return new JsonResult(viewModels);
        }

        [HttpPost]
        [AppAuthorize]
        public async Task<IActionResult> PostAsync(IntegrationLogViewModel model)
        {
            var result = await _integrationLogService.CreateAsync(model);
            return Ok();
        }
    }
}
