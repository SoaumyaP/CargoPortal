using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities;
using Groove.SP.Persistence.Contexts;
using Groove.SP.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Groove.SP.Persistence.Repositories
{
    public class UserProfileRepository : Repository<SpContext, UserProfileModel>, IUserProfileRepository
    {
        public UserProfileRepository(SpContext context)
            : base(context)
        { }

        public async Task<IEnumerable<RoleModel>> GetRoleByUserNameAsync(string username)
        {
            var result = await this.GetQueryable()
                                .Include(u => u.UserRoles)
                                .ThenInclude(r => r.Role.RolePermissions)
                                .SingleOrDefaultAsync(r => r.Username == username);

            return result.UserRoles.Select(x=> x.Role).ToList();
        }
    }
}
