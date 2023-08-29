using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities;
using Groove.SP.Persistence.Contexts;
using Groove.SP.Persistence.Repositories.Base;

namespace Groove.SP.Persistence.Repositories
{
    public class ShipmentContactRepository : Repository<SpContext, ShipmentContactModel>, IShipmentContactRepository
    {
        public ShipmentContactRepository(SpContext context)
            : base(context)
        {}
    }
}
