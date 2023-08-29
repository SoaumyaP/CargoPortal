using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities;
using Groove.SP.Persistence.Contexts;
using Groove.SP.Persistence.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Persistence.Repositories
{
    public class UserRoleRepository : Repository<SpContext, UserRoleModel>, IUserRoleRepository
    {
        public UserRoleRepository(SpContext context) : base(context)
        {

        }
    }
}
