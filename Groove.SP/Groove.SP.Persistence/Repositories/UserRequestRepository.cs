using System.Linq;

using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities;
using Groove.SP.Persistence.Contexts;
using Groove.SP.Persistence.Repositories.Base;

namespace Groove.SP.Persistence.Repositories
{
    public class UserRequestRepository : Repository<SpContext, UserRequestModel>, IUserRequestRepository
    {
        public UserRequestRepository(SpContext context)
            : base(context)
        { }

        protected override IQueryable<UserRequestModel> GetQueryable()
        {
            IQueryable<UserRequestModel> query = Context.Set<UserRequestModel>();
            return query;
        }
    }
}
