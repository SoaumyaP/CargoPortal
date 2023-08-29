using Groove.CSFE.Application.Common;
using IdentityModel;
using System;
using System.Security.Claims;

namespace Groove.CSFE.CommonAPI.Controllers
{
    public class ControllerBase : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        public IdentityInfo CurrentUser => new IdentityInfo()
        {
            Id = User.FindFirstValue(JwtClaimTypes.Id),
            Username = User.FindFirstValue(JwtClaimTypes.PreferredUserName),
            Email = User.FindFirstValue(JwtClaimTypes.Email),
            Name = User.FindFirstValue(JwtClaimTypes.Name),
            Phone = User.FindFirstValue(JwtClaimTypes.PhoneNumber),
            CompanyName = User.FindFirstValue("company_name"),
            IsInternal = Boolean.TryParse(User.FindFirstValue("is_internal"), out bool isInternal) ? isInternal : false
        };
    }
}
