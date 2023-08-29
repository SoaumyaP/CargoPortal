using Groove.CSFE.Application.Interfaces.Repositories;
using Groove.CSFE.Core.Entities;
using Groove.CSFE.Persistence.Contexts;
using Groove.CSFE.Persistence.Repositories.Base;

namespace Groove.CSFE.Persistence.Repositories
{
    public class CountryRepository : Repository<CsfeContext, CountryModel>, ICountryRepository
    {
        public CountryRepository(CsfeContext context)
            : base(context)
        {}
    }
}
