using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Entities.Cruise;
using Groove.SP.Persistence.Contexts;
using Groove.SP.Persistence.Repositories.Base;

namespace Groove.SP.Persistence.Repositories
{
    public class CruiseOrderRepository : Repository<SpContext, CruiseOrderModel>, ICruiseOrderRepository
    {
        public CruiseOrderRepository(SpContext context)
            : base(context)
        {}
    }
}
