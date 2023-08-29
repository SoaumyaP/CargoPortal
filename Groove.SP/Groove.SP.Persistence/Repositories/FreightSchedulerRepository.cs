using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities;
using Groove.SP.Persistence.Contexts;
using Groove.SP.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Threading.Tasks;

namespace Groove.SP.Persistence.Repositories
{
    public class FreightSchedulerRepository : Repository<SpContext, FreightSchedulerModel>, IFreightSchedulerRepository
    {
        public FreightSchedulerRepository(SpContext context) : base(context)
        {
        }

    }
}