using Groove.CSFE.Application.Interfaces.Repositories;
using Groove.CSFE.Core.Entities;
using Groove.CSFE.Persistence.Contexts;
using Groove.CSFE.Persistence.Repositories.Base;

namespace Groove.CSFE.Persistence.Repositories
{
    public class UserOfficeRepository : Repository<CsfeContext, UserOfficeModel>, IUserOfficeRepository
    {
        public UserOfficeRepository(CsfeContext context)
            : base(context)
        {

        }
    }
}
