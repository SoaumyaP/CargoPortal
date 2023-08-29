using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities;
using Groove.SP.Persistence.Contexts;
using Groove.SP.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Threading.Tasks;

namespace Groove.SP.Persistence.Repositories
{
    public class ConsolidationRepository : Repository<SpContext, ConsolidationModel>, IConsolidationRepository
    {
        public ConsolidationRepository(SpContext context)
            : base(context)
        { }

        public async Task<string> GetNextLoadPlanIDSequenceValueAsync()
        {
            using (var command = Context.Database.GetDbConnection().CreateCommand())
            {
                command.Transaction = Context.Database.CurrentTransaction?.GetDbTransaction();
                command.CommandText = "SELECT NEXT VALUE FOR [dbo].SequenceConsolidationLoadPlanID";
                Context.Database.OpenConnection();
                var nextValue = await command.ExecuteScalarAsync();

                return $"{nextValue:0000}";
            }
        }

        public async Task ResetLoadPlanIDSequenceValueAsync()
        {
            await Context.Database.ExecuteSqlRawAsync("ALTER SEQUENCE dbo.SequenceConsolidationLoadPlanID RESTART WITH 1");
        }
    }
}
