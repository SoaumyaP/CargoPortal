using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.Interfaces.Repositories
{
    public interface IUserProfileRepository : IRepository<UserProfileModel>
    {
        Task<IEnumerable<RoleModel>> GetRoleByUserNameAsync(string username);
    }
}
