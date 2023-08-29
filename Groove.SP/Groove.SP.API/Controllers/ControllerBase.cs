using System.Security.Claims;
using Groove.SP.API.Models;
using Groove.SP.Application.Common;

using IdentityModel;

namespace Groove.SP.API.Controllers
{
    public class ControllerBase : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        public IdentityInfo CurrentUser => new IdentityInfo
        {
            Id = User.FindFirstValue(JwtClaimTypes.Id),
            Username = User.FindFirstValue(JwtClaimTypes.PreferredUserName),
            Email = User.FindFirstValue(JwtClaimTypes.Email),
            Name = User.FindFirstValue(JwtClaimTypes.Name),
            Phone = User.FindFirstValue(JwtClaimTypes.PhoneNumber),
            CompanyName = User.FindFirstValue("company_name"),
            CompanyAddress = User.FindFirstValue("company_address"),
            IsInternal = bool.TryParse(User.FindFirstValue("is_internal"), out bool isInternal) && isInternal,

            //Business user information
            OrganizationId = long.TryParse(User.FindFirstValue("org_id"), out long organizationId) ? organizationId : 0,
            PortalUserName = User.FindFirstValue("portalu_name"),
            UserRoleId = long.TryParse(User.FindFirstValue("urole_id"), out long userRoleId) ? userRoleId : 0,
            UserRoleName = User.FindFirstValue("urole_name"),
            OPContactEmail = User.FindFirstValue("op_email"),
            OPContactName = User.FindFirstValue("op_name"),
            OPCountryId = long.TryParse(User.FindFirstValue("op_countryid"), out long countryId) ? countryId : 0,
            OPLocation = User.FindFirstValue("op_location"),
            TaxpayerId = User.FindFirstValue("taxpayerid"),

            IsUserRoleSwitch = !string.IsNullOrEmpty(User.FindFirstValue(AppConstants.SECURITY_USER_ROLE_SWITCH)) ? bool.Parse(User.FindFirstValue(AppConstants.SECURITY_USER_ROLE_SWITCH)) : false

        };
    }
}
