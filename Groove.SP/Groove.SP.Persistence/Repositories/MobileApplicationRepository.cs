
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities.Mobile;
using Groove.SP.Persistence.Contexts;
using Groove.SP.Persistence.Repositories.Base;

namespace Groove.SP.Persistence.Repositories
{
    public class MobileApplicationRepository : Repository<SpContext, MobileApplicationModel>, IMobileApplicationRepository
    {
        public MobileApplicationRepository(SpContext context) : base(context)
        {
        }
    }
}
