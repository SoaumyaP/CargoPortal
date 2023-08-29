using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Groove.SP.API.Filters;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.ImportData.Services.Interfaces;
using Groove.SP.Application.Permissions.Services.Interfaces;
using Groove.SP.Application.Users.Services.Interfaces;
using Groove.SP.Application.Users.ViewModels;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Data;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE.Models;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;
        private readonly IPermissionService _permissionService;
        private readonly IImportDataProgressService _importDataProgressService;

        public UsersController(
            IUserProfileService userProfileService,
            IPermissionService permissionService,
            IImportDataProgressService importDataProgressService)
        {
            _userProfileService = userProfileService;
            _permissionService = permissionService;
            _importDataProgressService = importDataProgressService;
        }

        [Route("{id}")]
        [HttpGet]
        [AppAuthorize(AppPermissions.User_UserDetail)]
        public async Task<IActionResult> Get(long id)
        {
            var viewModel = await _userProfileService.GetAsync(id);
            return new JsonResult(viewModel);
        }

        [Route("Current")]
        [HttpGet]
        public async Task<IActionResult> GetCurrentUser()
        {
            // Get current user by claim
            var userName = CurrentUser.Username;
           
            if (string.IsNullOrWhiteSpace(userName))
                return new JsonResult(null);

            var viewModel = await _userProfileService.GetAsync(CurrentUser);
            if (viewModel != null)
            {
                if (viewModel.Status == UserStatus.WaitForConfirm)
                {
                    await _userProfileService.ActivateUserAsync(viewModel.Username);
                }
                // Will get permissions by role if in user role switch mode
                var isUserRoleSwitch = CurrentUser.IsUserRoleSwitch;
                var userRoleId = CurrentUser.UserRoleId;
                if (isUserRoleSwitch)
                {
                    viewModel.Permissions = (await _permissionService.GetRolePermissions(userRoleId)).ToList();
                }
                else
                {
                    viewModel.Permissions = (await _permissionService.GetUserPermissions(CurrentUser.Username)).ToList();
                }
                return new JsonResult(viewModel);
            }

            // If not existed in DB, create new profile with claim info
            // Company Address cut off for the first 200 characters
            var companyAddress = CurrentUser.CompanyAddress;
            var splitAddress = (companyAddress.Length <= 200 ? companyAddress : companyAddress.Substring(0, 200)).SplitAddressBySize();
            var user = new UserProfileViewModel()
            {
                Username = userName,
                Email = CurrentUser.Email,
                Name = CurrentUser.Name,
                Phone = CurrentUser.Phone,
                CompanyName = CurrentUser.CompanyName,
                CompanyAddressLine1 = splitAddress.TryGetValue(1, out string addressLine1) ? addressLine1 : null,
                CompanyAddressLine2 = splitAddress.TryGetValue(2, out string addressLine2) ? addressLine2 : null,
                CompanyAddressLine3 = splitAddress.TryGetValue(3, out string addressLine3) ? addressLine3 : null,
                CompanyAddressLine4 = splitAddress.TryGetValue(4, out string addressLine4) ? addressLine4 : null,
                CompanyWeChatOrWhatsApp = User.FindFirstValue("company_weChatOrWhatsApp"),
                Customer = User.FindFirstValue("customer") ?? "",
                OPContactEmail = CurrentUser.OPContactEmail,
                OPContactName = CurrentUser.OPContactName,
                OPCountryId = CurrentUser.OPCountryId,
                OPLocationName = CurrentUser.OPLocation,
                TaxpayerId = CurrentUser.TaxpayerId
            };
            viewModel = await _userProfileService.CreateUserAsync(user, CurrentUser.IsInternal);
            viewModel.Permissions = (await _permissionService.GetUserPermissions(CurrentUser.Username)).ToList();

            return new JsonResult(viewModel);
        }

        [Route("ByOrganization/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetUsersByOrganizationId(long id)
        {
            var result = await _userProfileService.GetUsersByOrganizationIdAsync(id);
            return new JsonResult(result);
        }

        [Route("CheckExistsUser")]
        [HttpGet]
        public async Task<IActionResult> CheckExistsUser(string email)
        {
            var result = await _userProfileService.CheckExistsUser(email);
            return new JsonResult(result);
        }

        [HttpPut]
        [Route("{id}/Status")]
        [AppAuthorize]
        public async Task<IActionResult> UpdateUserStatusAsync(long id, [FromBody] UpdateUserStatusViewModel viewModel)
        {
            var result = await _userProfileService.UpdateUserStatusAsync(id, viewModel, CurrentUser.Username);
            return Ok(result);
        }

        [Route("UpdateStatusUsers/{organizationId}")]
        [HttpPut]
        [AppAuthorize(AppPermissions.Organization_Detail_Edit)]
        public async Task<IActionResult> UpdateStatusUsers(long organizationId, [FromBody] UserProfileViewModel model)
        {
            var username = CurrentUser.Username;
            var result = await _userProfileService.UpdateStatusUsersAsync(organizationId, model.Status, username);
            return new JsonResult(result);
        }

        [Route("UpdateOrganization/{organizationId}")]
        [HttpPut]
        public async Task<IActionResult> UpdateOrganization(long organizationId, [FromBody] UserProfileViewModel model)
        {
            var username = CurrentUser.Username;
            var result = await _userProfileService.UpdateOrganizationAsync(organizationId, model.OrganizationName, model.OrganizationType, username);
            return new JsonResult(result);
        }

        [HttpPost]
        [AppAuthorize]
        public async Task<IActionResult> Post([FromBody] UserProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _userProfileService.CreateUserAsync(model, CurrentUser.IsInternal);
                return new JsonResult(result);
            }
            return BadRequest();
        }

        [HttpPost("validateExcelImport")]
        [AppAuthorize]
        public async Task<IActionResult> ValidateExcelImportAsync([FromForm] IFormFile files)
        {
            if (files == null || files.Length < 0)
            {
                return BadRequest();
            }

            string userName = CurrentUser.Username;
            var importDataProgress = await _importDataProgressService.CreateAsync("Validate User From Excel", userName);
            BackgroundJob.Enqueue<IUserProfileService>(s => s.ValidateExcelImportAsync(files.GetAllBytes(), files.FileName, userName, importDataProgress.Id));

            return Ok(importDataProgress.Id);
        }

        [HttpPost("Import")]
        [AppAuthorize]
        public async Task<IActionResult> ImportAsync([FromForm] IFormFile files)
        {
            if (files == null || files.Length < 0)
            {
                return BadRequest();
            }

            string userName = CurrentUser.Username;
            string email = CurrentUser.Email;
            string name = CurrentUser.Name;
            var importDataProgress = await _importDataProgressService.CreateAsync("Import User From Excel", userName);
            BackgroundJob.Enqueue<IUserProfileService>(s => s.ImportExcelSilentAsync(files.GetAllBytes(), files.FileName, userName, email, name, importDataProgress.Id));

            return Ok(importDataProgress.Id);
        }

        [Route("external")]
        [HttpPost]
        [AppAuthorize]
        public async Task<IActionResult> AddNewExternalUser([FromBody] UserProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var isExists = await _userProfileService.CheckExistsUser(model.Email);
                if (!isExists)
                {
                    var result = await _userProfileService.CreateExternalUserAsync(model);
                    return new JsonResult(result);
                }
                return BadRequest();
            }
            return BadRequest();
        }

        [HttpPut]
        [Route("Current")]
        [AppAuthorize]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UserProfileViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var username = CurrentUser.Username;
                var result = await _userProfileService.UpdateCurrentUserAsync(viewModel, username);
                return new JsonResult(result);
            }
            return BadRequest();
        }

        [HttpPut]
        [Route("{id}")]
        [AppAuthorize]
        public async Task<IActionResult> Update(long id, [FromBody] UserProfileViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var username = CurrentUser.Username;
                var result = await _userProfileService.UpdateAsync(id, viewModel, username);
                return new JsonResult(result);
            }
            return BadRequest();
        }

        [HttpGet]
        [Route("Search")]
        [AppAuthorize(AppPermissions.User_UserList)]
        public async Task<IActionResult> Search([DataSourceRequest] DataSourceRequest request)
        {
            var viewModels = await _userProfileService.ListAsync(request);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("Selections")]
        [AppAuthorize(AppPermissions.User_UserList)]
        public async Task<IActionResult> GetMultipleSelectionAsync([FromQuery] string searchEmail = "")
        {
            var viewModels = await _userProfileService.GetSelectionAsync(searchEmail);
            return new JsonResult(viewModels);
        }

        [HttpPut]
        [Route("current/lastSignIn")]
        public async Task<IActionResult> UpdateLastSignInDate()
        {
            var username = CurrentUser.Username;
            await _userProfileService.UpdateLastSignInDateAsync(username);
            return Ok();
        }

        [HttpPost]
        [Route("Current/SyncUserTracking")]
        public async Task<IActionResult> SyncUserTracking(IEnumerable<UserAuditLogViewModel> data)
        {
            var username = CurrentUser.Username;
            await _userProfileService.SyncUserTracking(username, data);
            return Ok();
        }

        [HttpGet]
        [Route("Trace")]
        public async Task<IActionResult> TraceUserByEmailSearching([DataSourceRequest] DataSourceRequest request, string email, string action = "searching")
        {
            switch (action.ToLower())
            {
                case "searching":
                    var searchingData = await _userProfileService.TraceUserByEmailSearchingAsync(request, email);
                    return new JsonResult(searchingData);
                case "count":
                    var count = await _userProfileService.TraceUserByEmailTotalCountAsync(email);
                    return new JsonResult(count);
                default:
                    break;
            }
            return Ok();
        }

        [HttpPost]
        [Route("HasPermissions")]
        [AppAuthorize]
        public async Task<IActionResult> HasPermissions()
        {
            return Ok();
        }

        [HttpGet]
        [Route("ResetPermissionCache")]
        [AppAuthorize]
        public IActionResult ResetPermissionCache()
        {
            _permissionService.RemovePermissionsCacheOfUser(CurrentUser.Username);
            return Ok();
        }

        [HttpGet]
        [Route("{username}/organization")]
        [AppAuthorize]
        public async Task<IActionResult> GetUserOrganizationByUsernameAsync(string username)
        {
            var userProfile = await _userProfileService.GetByUsernameAsync(username);
            var result = new UserOrganizationProfileViewModel()
            {
                OrganizationId = userProfile.OrganizationId,
                OrganizationName = userProfile.OrganizationName,
                OrganizationCode = userProfile.OrganizationCode,
                OrganizationRoleId = userProfile.OrganizationRoleId,
                OrganizationType = userProfile.OrganizationType
            };
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [AppAuthorize]
        public async Task<IActionResult> DeleteUserAsync(long id)
        {
            await _userProfileService.RemoveExternalUserCompletelyAsync(id, CurrentUser);
            return Ok();
        }

        [HttpPost]
        [Route("{id}/send-activation-email")]
        [AppAuthorize]
        public async Task<IActionResult> SendActivationEmailAsync(long id)
        {
            await _userProfileService.SendActivationEmail(id);
            return Ok();
        }
    }
}