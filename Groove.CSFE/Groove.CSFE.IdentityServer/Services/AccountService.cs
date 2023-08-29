using Dapper;
using Groove.CSFE.IdentityServer.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.CSFE.IdentityServer.Services
{
    public class AccountService : IAccountService
    {
        private readonly string _csPortalDatabaseConnectionString;

        public AccountService(IConfiguration configuration)
        {
            _csPortalDatabaseConnectionString = configuration.GetConnectionString("CSPortalDatabase");

        }
        public async Task<bool> CheckIsInRoleByEmailAsync(string userName, params int[] roles)
        {
            if (roles == null || !roles.Any())
            {
                return false;
            }
            string isAdmin;
            string rolesFiltering = string.Join(", ", roles);
            var parameters = new { userName = userName };
            using (IDbConnection db = new SqlConnection(_csPortalDatabaseConnectionString))
            {
                var query = @$"
                            SELECT 1
                            FROM UserRoles UR WITH(NOLOCK)
                            WHERE UR.RoleId IN ({rolesFiltering}) AND 
	                            EXISTS ( SELECT 1 
	                            FROM UserProfiles UP WITH(NOLOCK) 
	                            WHERE UP.Id = UR.UserId
		                            AND UP.Email = @userName
	                            )
                            ";

                isAdmin = db.QueryFirst<string>(query, parameters);
            }

            return !string.IsNullOrEmpty(isAdmin);
        }
    }
}
