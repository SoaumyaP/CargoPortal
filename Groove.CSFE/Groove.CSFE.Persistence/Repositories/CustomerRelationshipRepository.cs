using Groove.CSFE.Application.Interfaces.Repositories;
using Groove.CSFE.Core.Entities;
using Groove.CSFE.Persistence.Contexts;
using Groove.CSFE.Persistence.Repositories.Base;

namespace Groove.CSFE.Persistence.Repositories
{
    public class CustomerRelationshipRepository : Repository<CsfeContext, CustomerRelationshipModel>, ICustomerRelationshipRepository
    {
        public CustomerRelationshipRepository(CsfeContext context) : base(context)
        {
        }
    }
}