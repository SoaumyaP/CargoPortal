using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Groove.SP.Application.Permissions.Services.Interfaces;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionsController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }
        
        [HttpGet]
        [Route("List")]
        public async Task<IActionResult> Get()
        {
            var result = await _permissionService.GetAllAsync();
            return new JsonResult(result);
        }
    }
}
