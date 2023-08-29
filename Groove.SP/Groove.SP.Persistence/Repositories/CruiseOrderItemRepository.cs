using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities.Cruise;
using Groove.SP.Persistence.Contexts;
using Groove.SP.Persistence.Repositories.Base;

namespace Groove.SP.Persistence.Repositories
{
    public class CruiseOrderItemRepository : Repository<SpContext, CruiseOrderItemModel>, ICruiseOrderItemRepository
    {
        public CruiseOrderItemRepository(SpContext context)
            : base(context)
        {}
    }
}
