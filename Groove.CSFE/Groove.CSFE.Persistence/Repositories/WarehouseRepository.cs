using Groove.CSFE.Application.Interfaces.Repositories;
using Groove.CSFE.Core.Entities;
using Groove.CSFE.Persistence.Contexts;
using Groove.CSFE.Persistence.Repositories.Base;

namespace Groove.CSFE.Persistence.Repositories
{
    public class WarehouseRepository : Repository<CsfeContext, WarehouseModel>, IRepository<WarehouseModel>
    {
        public WarehouseRepository(CsfeContext context) : base(context)
        {
        }
    }
}
