using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Groove.SP.Application.Users.Services.Interfaces;
using Groove.SP.Application.Users.ViewModels;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.Permissions.Services.Interfaces;
using Groove.SP.Core.Models;
using Microsoft.Extensions.Options;
using Groove.SP.Application.Exceptions;
using Groove.SP.Infrastructure.EmailSender.Models;
using Groove.SP.Application.ApplicationBackgroundJob;
using Hangfire;
using Groove.SP.Core.Data;
using Groove.SP.API.Filters;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRequestsController : ControllerBase
    {
        private readonly IUserRequestService _userRequestService;
        private readonly IPermissionService _permissionService;
        private readonly AppConfig _appConfig;

        public UserRequestsController(IUserRequestService userRequestService, 
            IPermissionService permissionService,
            IOptions<AppConfig> appConfig)
        {
            _userRequestService = userRequestService;
            _permissionService = permissionService;
            _appConfig = appConfig.Value;
        }

        [HttpGet]
        [Route("{id}")]
        [AppAuthorize(AppPermissions.User_RequestDetail)]
        public async Task<IActionResult> Get(long id)
        {
            var viewModel = await _userRequestService.GetAsync(id);
            return new JsonResult(viewModel);
        }

        [HttpGet]
        [Route("Search")]
        [AppAuthorize(AppPermissions.User_RequestList)]
        public async Task<IActionResult> Search([DataSourceRequest] DataSourceRequest request)
        {
            var viewModels = await _userRequestService.ListAsync(request);
            return new JsonResult(viewModels);
        }

        [HttpPut]
        [Route("{id}/Approve")]
        [AppAuthorize(AppPermissions.User_RequestDetail_Edit)]
        public async Task<IActionResult> Approve(long id, [FromBody] UserRequestViewModel viewModel)
        {
            var username = await _userRequestService.ApproveAsync(id, viewModel, CurrentUser.Username);
            _permissionService.RemovePermissionsCacheOfUser(username);
            return Accepted();
        }

        [HttpPut]
        [Route("{id}/Reject")]
        [AppAuthorize(AppPermissions.User_RequestDetail_Edit)]
        public async Task<IActionResult> Reject(long id, [FromBody] UserRequestViewModel viewModel)
        {
            var username = await _userRequestService.RejectAsync(id, viewModel, CurrentUser.Username);
            _permissionService.RemovePermissionsCacheOfUser(username);
            return Accepted();
        }

        [HttpGet]
        [Route("{id}/Resubmit")]
        public async Task<IActionResult> Resubmit(long id)
        {
            var model = await _userRequestService.GetAsync(id);

            if (model.Status != UserStatus.Rejected)
            {
                throw new AppException("msg.cannotResubmit");
            }

            // Send resubmit email
            var resubmitUserEmailParameters = new ResubmitUserEmailParameters()
            {
                Email = model.Email,
                CompanyName = model.CompanyName,
                Name = model.Name,
                Phone = model.Phone,
                UserLink = $"{_appConfig.ClientUrl}/user-requests/edit/{model.Id}"
            };

            BackgroundJob.Enqueue<SendMailBackgroundJobs>(x => x.SendMailAsync($"Account Registration - Resubmit Request Rejected (Admin) {model.Email}",
                "AccountRejectedResubmitForApprove", resubmitUserEmailParameters, _appConfig.AdminAccount, 
                $" Shipment Portal: Email App Account Registration - Resubmit Request Rejected (Admin)"));

            return Ok();
        }
    }
}
