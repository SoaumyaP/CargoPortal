using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities.Cruise;
using Groove.SP.Persistence.Contexts;
using Groove.SP.Persistence.Repositories.Base;

namespace Groove.SP.Persistence.Repositories
{
    public class CruiseOrderWarehouseInfoRepository : Repository<SpContext, CruiseOrderWarehouseInfoModel>, ICruiseOrderWarehouseInfoRepository
    {
        public CruiseOrderWarehouseInfoRepository(SpContext context)
            : base(context)
        { }
    }
}
