using System.Threading.Tasks;
using Groove.SP.Application.GlobalIdActivity.ViewModels;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities;
using Groove.SP.Persistence.Contexts;
using Groove.SP.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Groove.SP.Persistence.Repositories
{
    public class GlobalIdActivityRepository : Repository<SpContext, GlobalIdActivityModel>, IGlobalIdActivityRepository
    {
        public GlobalIdActivityRepository(SpContext context) : base(context)
        {
        }
    }
}