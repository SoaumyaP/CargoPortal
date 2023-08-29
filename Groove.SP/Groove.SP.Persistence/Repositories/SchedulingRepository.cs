using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities;
using Groove.SP.Persistence.Contexts;
using Groove.SP.Persistence.Repositories.Base;

namespace Groove.SP.Persistence.Repositories
{
    public class SchedulingRepository : Repository<SpContext, SchedulingModel>, ISchedulingRepository
    {
        public SchedulingRepository(SpContext context)
            : base(context)
        { }
    }
}
