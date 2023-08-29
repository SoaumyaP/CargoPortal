using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Dapper;
using Groove.CSFE.Core;
using Groove.CSFE.IdentityServer.Extensions;
using Groove.CSFE.IdentityServer.Models;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Groove.CSFE.IdentityServer.Services
{
    public class IdentityWithAdditionalClaimsProfileService : IProfileService
    {
        private readonly AzureAdConfig _azureAdConfig;
        private readonly string _csPortalDatabaseConnectionString;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public IdentityWithAdditionalClaimsProfileService(IOptions<AzureAdConfig> azureAdConfig, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _azureAdConfig = azureAdConfig.Value;
            _csPortalDatabaseConnectionString = configuration.GetConnectionString("CSPortalDatabase");
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var subjectClaims = context.Subject.Claims.ToList();

            var isPortalInternal = subjectClaims.Find(c => c.Type == "is_internal")?.Value;
            // if it is available, return as all claims available to work
            if (!string.IsNullOrEmpty(isPortalInternal))
            {
                return;
            }

            var email = subjectClaims.Find(c => c.Type == "emails") ?? subjectClaims.Find(c => c.Type == JwtClaimTypes.PreferredUserName);
            var phone = subjectClaims.Find(c => c.Type == JwtClaimTypes.PhoneNumber) ?? subjectClaims.Find(c => c.Type == "extension_PhoneNumber");
            var companyName = subjectClaims.Find(c => c.Type == "extension_CompanyName") ?? subjectClaims.Find(c => c.Type == "company_name");
            var companyAddress = subjectClaims.Find(c => c.Type == "extension_CompanyAddress") ?? subjectClaims.Find(c => c.Type == "company_address");
            var companyWeChatOrWhatsApp = subjectClaims.Find(c => c.Type == "extension_CompanyWeChatOrWhatsApp") ?? subjectClaims.Find(c => c.Type == "company_weChatOrWhatsApp");
            var customer = subjectClaims.Find(c => c.Type == "extension_Customer") ?? subjectClaims.Find(c => c.Type == "customer");
            var opEmail = subjectClaims.Find(c => c.Type == "extension_OP_EmailAddress") ?? subjectClaims.Find(c => c.Type == "op_email");
            var opName = subjectClaims.Find(c => c.Type == "extension_OP_Name") ?? subjectClaims.Find(c => c.Type == "op_name");
            var opCountryId = subjectClaims.Find(c => c.Type == "extension_OP_CountryID") ?? subjectClaims.Find(c => c.Type == "op_countryid");
            var opLocation = subjectClaims.Find(c => c.Type == "extension_OP_Location") ?? subjectClaims.Find(c => c.Type == "op_location");
            var opTaxpayerId = subjectClaims.Find(c => c.Type == "extension_TaxpayerID") ?? subjectClaims.Find(c => c.Type == "taxpayerid");

            // tenant id, Azure AD returns tenant id in access token.
            var tenantId = subjectClaims.Find(c => c.Type == "tid")?.Value;

            // compare to tenant id in configuration. If matched, it is from Azure AD for internal users, else Azure B2C external
            var isAzureInternal = _azureAdConfig.CheckInternalUser(tenantId);

            if (string.IsNullOrEmpty(email?.Value))
                return;

            // call to database CSPortal to obtain more business user information
            BusinessUserInformation businessUserInformation;
            using (IDbConnection db = new SqlConnection(_csPortalDatabaseConnectionString))
            {
                var query = @"
                            SELECT UP.Email AS [UserEmail],
                            UP.Name AS [PortalUserName],
		                    UP.OrganizationId AS [OrganizationId],
                            UP.IsInternal AS [IsInternal],
		                    UR.RoleId AS [UserRoleId],
		                    R.[Name] AS [UserRoleName]
                            FROM UserProfiles UP
                            INNER JOIN UserRoles UR ON UP.Id = UR.UserId
                            INNER JOIN Roles R ON UR.RoleId = R.Id
                            WHERE UP.Email = '" + email.Value + "'";

                businessUserInformation = db.Query<BusinessUserInformation>(query).FirstOrDefault();                
            }

            var azureNameClaim = subjectClaims.Find(c => c.Type == JwtClaimTypes.Name);

            var claims = new List<Claim>
            {
                azureNameClaim,
                new Claim(JwtClaimTypes.PreferredUserName, email?.Value),
                new Claim(JwtClaimTypes.Email, email?.Value),
                new Claim(JwtClaimTypes.PhoneNumber, phone?.Value ?? ""),
                new Claim("company_name", companyName?.Value ?? ""),
                new Claim("company_address", companyAddress?.Value ?? ""),
                new Claim("company_weChatOrWhatsApp", companyWeChatOrWhatsApp?.Value ?? ""),
                new Claim("customer", customer?.Value ?? ""),

                new Claim("op_email", opEmail?.Value ?? ""),
                new Claim("op_name", opName?.Value ?? ""),
                new Claim("op_countryid", opCountryId?.Value ?? ""),
                new Claim("op_location", opLocation?.Value ?? ""),
                new Claim("taxpayerid", opTaxpayerId?.Value ?? ""),
                // Just in case no name of user in database, try to get from Azure
                new Claim("portalu_name", (businessUserInformation?.PortalUserName ?? azureNameClaim.Value).ToString())               

            };

            // To get session if it is in switch mode
            var isUserRoleSwitchMode = _httpContextAccessor.HttpContext.Session.GetString(AppConstants.SECURITY_USER_ROLE_SWITCH);

            // If it is in user role switch mode
            if (bool.TrueString.Equals(isUserRoleSwitchMode, System.StringComparison.InvariantCultureIgnoreCase))
            {
                
                var roleId = _httpContextAccessor.HttpContext.Session.GetString("roleId");
                var organizationId = _httpContextAccessor.HttpContext.Session.GetString("organizationId");

                string roleName;
                using (IDbConnection db = new SqlConnection(_csPortalDatabaseConnectionString))
                {
                    var query = @$"
                            SELECT [Name]
                            FROM [dbo].[Roles]
                            WHERE Id = {roleId}
                            ";

                    roleName = db.QueryFirst<string>(query);
                }

                // Must be False in switch mode
                claims.Add(new Claim("is_internal", bool.FalseString));
                claims.Add(new Claim("org_id", organizationId));
                claims.Add(new Claim("urole_id", roleId));
                claims.Add(new Claim("urole_name", roleName));
                claims.Add(new Claim(AppConstants.SECURITY_USER_ROLE_SWITCH, bool.TrueString));

            }
            else
            {
                // adding new claims on business user information here
                // in case there is not user profile in database yet, get value from Azure
                claims.Add(new Claim("is_internal", (businessUserInformation?.IsInternal ?? isAzureInternal).ToString()));
                // if not found returned as Guest
                claims.Add(new Claim("org_id", (businessUserInformation?.OrganizationId ?? 0).ToString()));

                claims.Add(new Claim("urole_id", (businessUserInformation?.UserRoleId ?? (int)Role.Guest).ToString()));
                claims.Add(new Claim("urole_name", businessUserInformation?.UserRoleName ?? Role.Guest.ToString()));
            }

            context.IssuedClaims = claims;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var identity = context.Subject.Identity;
            context.IsActive = identity != null && identity.IsAuthenticated;
        }
    }
}