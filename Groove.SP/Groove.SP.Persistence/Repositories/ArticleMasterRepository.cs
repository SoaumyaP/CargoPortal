using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities;
using Groove.SP.Persistence.Contexts;
using Groove.SP.Persistence.Repositories.Base;

namespace Groove.SP.Persistence.Repositories;
public class ArticleMasterRepository : Repository<SpContext, ArticleMasterModel>, IArticleMasterRepository
{
    public ArticleMasterRepository(SpContext context)
            : base(context)
    { }
}