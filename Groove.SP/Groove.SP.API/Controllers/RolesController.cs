using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Groove.SP.Application.Users.Services.Interfaces;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.Users.ViewModels;
using Groove.SP.Core.Data;
using Groove.SP.API.Filters;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }
        
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _roleService.GetAllAsync();
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{id}")]
        [AppAuthorize(AppPermissions.User_RoleDetail)]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _roleService.GetAsync(id);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("Official")]
        public async Task<IActionResult> GetOfficial()
        {
            var result = await _roleService.GetOfficialAsync();
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("Search")]
        [AppAuthorize(AppPermissions.User_RoleList)]
        public async Task<IActionResult> Search([DataSourceRequest] DataSourceRequest request)
        {
            var viewModels = await _roleService.ListAsync(request);
            return new JsonResult(viewModels);
        }

        [HttpPut]
        [Route("{id}")]
        [AppAuthorize(AppPermissions.User_RoleDetail_Edit)]
        public async Task<IActionResult> Update(long id, [FromBody] RoleViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var username = CurrentUser.Username;
                var result = await _roleService.UpdateAsync(viewModel, username);
                return new JsonResult(result);
            }
            return BadRequest();
        }
    }
}
