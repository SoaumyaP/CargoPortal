using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities;
using Groove.SP.Persistence.Contexts;
using Groove.SP.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Threading.Tasks;

namespace Groove.SP.Persistence.Repositories
{
    public class BuyerApprovalRepository : Repository<SpContext, BuyerApprovalModel>, IBuyerApprovalRepository
    {
        public BuyerApprovalRepository(SpContext context) : base(context)
        {
        }

        public async Task<string> GetNextBuyerApprovalSequenceValueAsync()
        {
            using (var command = Context.Database.GetDbConnection().CreateCommand())
            {
                command.Transaction = Context.Database.CurrentTransaction?.GetDbTransaction();
                command.CommandText = "SELECT NEXT VALUE FOR [dbo].SequenceBuyerApprovalReferenceNumber";
                Context.Database.OpenConnection();
                var nextValue = (int)await command.ExecuteScalarAsync();

                return $"{nextValue:00000}";
            }
        }

        public async Task ResetBuyerApprovalSequenceValueAsync()
        {
            await Context.Database.ExecuteSqlRawAsync("ALTER SEQUENCE dbo.SequenceBuyerApprovalReferenceNumber RESTART WITH 1");
        }
    }
}