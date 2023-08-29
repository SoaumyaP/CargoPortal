using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities;
using Groove.SP.Persistence.Contexts;
using Groove.SP.Persistence.Repositories.Base;

namespace Groove.SP.Persistence.Repositories
{
    public class POFulfillmentShortshipOrderRepository : Repository<SpContext, POFulfillmentShortshipOrderModel>, IPOFulfillmentShortshipOrderRepository
    {
        public POFulfillmentShortshipOrderRepository(SpContext context) : base(context)
        {
        }
    }
}