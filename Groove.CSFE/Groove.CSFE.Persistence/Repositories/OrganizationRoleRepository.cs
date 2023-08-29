using Groove.CSFE.Application.Interfaces.Repositories;
using Groove.CSFE.Core.Entities;
using Groove.CSFE.Persistence.Contexts;
using Groove.CSFE.Persistence.Repositories.Base;

namespace Groove.CSFE.Persistence.Repositories
{
    public class OrganizationRoleRepository : Repository<CsfeContext, OrganizationRoleModel>, IOrganizationRoleRepository
    {
        public OrganizationRoleRepository(CsfeContext context)
            : base(context)
        { }
    }
}
