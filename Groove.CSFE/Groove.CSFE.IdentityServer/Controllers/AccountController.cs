using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Groove.CSFE.Core;
using Groove.CSFE.IdentityServer.Services.Interfaces;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Groove.CSFE.IdentityServer.Controllers
{
    public class AccountController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;

        private const string LOGIN_TYPE_PARAM_NAME = "type";
        private const string INTERNAL_USER = "in";
        private const string EXTRNAL_USER = "ex";
        private readonly IAccountService _accountService;

        public AccountController(
            IIdentityServerInteractionService interaction,
            IAccountService accountService
            )
        {
            _interaction = interaction;
            _accountService = accountService;
        }

        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            return context.Parameters[LOGIN_TYPE_PARAM_NAME] != INTERNAL_USER ? SignIn(returnUrl) : InternalSignIn(returnUrl);
        }

        [HttpGet]
        public IActionResult LoginPage(string returnUrl = null)
        {
            return RedirectToAction("Index", "Home", new { returnUrl = returnUrl });
        }

        [HttpGet]
        public IActionResult SignIn(string returnUrl = null)
        {
            returnUrl = returnUrl ?? HttpContext.Request.Query["returnUrl"];
            var redirectUrl = Url.Action("SignedIn", "Account", new { ReturnUrl = returnUrl });
            return Challenge(
                new AuthenticationProperties() { RedirectUri = redirectUrl }, "B2C");
        }

        [HttpGet]
        public IActionResult InternalSignIn(string returnUrl = null)
        {
            returnUrl = returnUrl ?? HttpContext.Request.Query["returnUrl"];
            var redirectUrl = Url.Action("SignedIn", "Account", new { ReturnUrl = returnUrl });
            return Challenge(
                new AuthenticationProperties() { RedirectUri = redirectUrl }, "AD");
        }

        [HttpGet]
        public IActionResult SignedIn(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, remoteError);
                return View("Error");
            }

            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        /// <summary>
        /// To switch to specific role at organization
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> SwitchToUserRole(long organizationId, long roleId)
        {
            if (organizationId == 0 || roleId == 0)
            {
                return BadRequest();
            }

            var currentUser = HttpContext.User.Identity;
            // Azure AD
            var currentUserName = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "preferred_username")?.Value;
            // Azure B2C
            if (string.IsNullOrEmpty(currentUserName))
            {
                currentUserName = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "emails")?.Value;

            }

            if (!currentUser.IsAuthenticated || string.IsNullOrEmpty(currentUserName))
            {
                return Unauthorized();
            }

            // Need to check whether current is internal (Admin/CSR)
            var isAdmin = await _accountService.CheckIsInRoleByEmailAsync(currentUserName, (int)Role.SystemAdmin, (int)Role.CSR);
            if (!isAdmin)
            {
                return Forbid();
            }                    

            // To set data to current session
            HttpContext.Session.Set(AppConstants.SECURITY_USER_ROLE_SWITCH, Encoding.ASCII.GetBytes(bool.TrueString));
            HttpContext.Session.Set("organizationId", Encoding.ASCII.GetBytes(organizationId.ToString()));
            HttpContext.Session.Set("roleId", Encoding.ASCII.GetBytes(roleId.ToString()));
            return Accepted();

        }

        /// <summary>
        /// To switch off/exit pretending user role
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> SwitchOffUserRole()
        {
            var currentUser = HttpContext.User.Identity;

            // Azure AD
            var currentUserName = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "preferred_username")?.Value;
            // Azure B2C
            if (string.IsNullOrEmpty(currentUserName))
            {
                currentUserName = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "emails")?.Value;

            }

            if (!currentUser.IsAuthenticated || string.IsNullOrEmpty(currentUserName))
            {
                return Unauthorized();
            }

            // Need to check whether current is internal (Admin/CSR)
            var isAdmin = await _accountService.CheckIsInRoleByEmailAsync(currentUserName, (int)Role.SystemAdmin, (int)Role.CSR);
            if (!isAdmin)
            {
                return Forbid();
            }
            ClearUserRoleSwitchData();
            return Accepted();

        }

        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId, string type)
        {
            var logout = await _interaction.GetLogoutContextAsync(logoutId);
            return await (logout.Parameters[LOGIN_TYPE_PARAM_NAME] != INTERNAL_USER ? SignOut(logoutId) : InternalSignOut(logoutId));
        }

        [HttpGet]
        public async Task<IActionResult> SignOut(string logoutId)
        {
            ClearUserRoleSwitchData();

            // sign out local identity server
            await HttpContext.SignOutAsync();

            // sign out B2C
            var callbackUrl = Url.Action(nameof(SignedOut), "Account", new { logoutId = logoutId }, protocol: Request.Scheme);
            return SignOut(new AuthenticationProperties { RedirectUri = callbackUrl },
                CookieAuthenticationDefaults.AuthenticationScheme, "B2C");
        }

        [HttpGet]
        public async Task<IActionResult> InternalSignOut(string logoutId)
        {
            ClearUserRoleSwitchData();

            // sign out local identity server
            await HttpContext.SignOutAsync();

            // sign out AD
            var callbackUrl = Url.Action(nameof(SignedOut), "Account", new { logoutId = logoutId }, protocol: Request.Scheme);
            return SignOut(new AuthenticationProperties { RedirectUri = callbackUrl },
                CookieAuthenticationDefaults.AuthenticationScheme, "AD");
        }

        [HttpGet]
        public async Task<IActionResult> SignedOut(string logoutId)
        {
            if (User.Identity.IsAuthenticated)
            {
                // Redirect to home page if the user is authenticated.
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            if (!string.IsNullOrWhiteSpace(logout?.PostLogoutRedirectUri))
            {
                return Redirect(logout?.PostLogoutRedirectUri);
            }

            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            var redirectUrl = Url.Action(nameof(HomeController.Index), "Home");
            return Challenge(new AuthenticationProperties { RedirectUri = redirectUrl }, "B2C-RSPW");
        }

        [HttpGet]
        public IActionResult ResetPasswordSuccess()
        {
            return View();
        }

        /// <summary>
        /// To clear data in user role switch mode
        /// </summary>
        private void ClearUserRoleSwitchData()
        {
            HttpContext.Session.Remove(AppConstants.SECURITY_USER_ROLE_SWITCH);
            HttpContext.Session.Remove("organizationId");
            HttpContext.Session.Remove("roleId");
        }
    }
}