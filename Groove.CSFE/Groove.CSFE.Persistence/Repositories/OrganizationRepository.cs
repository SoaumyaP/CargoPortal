using Groove.CSFE.Application.Interfaces.Repositories;
using Groove.CSFE.Core.Entities;
using Groove.CSFE.Persistence.Contexts;
using Groove.CSFE.Persistence.Repositories.Base;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Groove.CSFE.Persistence.Repositories
{
    public class OrganizationRepository: Repository<CsfeContext, OrganizationModel>, IOrganizationRepository
    {
        public OrganizationRepository(CsfeContext context)
            :base(context)
        {

        }

        public async Task<string> GetNextNumberAsync()
        {
            using (var command = Context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "SELECT NEXT VALUE FOR [dbo].SequenceOrganizationNumber";
                Context.Database.OpenConnection();
                var nextValue = (int)await command.ExecuteScalarAsync();
                var nextNumber = $"ORG{nextValue:0000}";

                return nextNumber;
            }
        }

        public async Task<int> ReserveSequenceNumberAsync(int quantityToReserve)
        {
            var sql = @"
                        DECLARE @currentOrganizationNumber INT;

                        -- Get current Sequence value
                        SELECT @currentOrganizationNumber = CAST(current_value AS INT) FROM sys.sequences WHERE name = 'SequenceOrganizationNumber';

                        -- Seed Sequence to new value
                        DECLARE @resetSQL nvarchar(255) = 'ALTER SEQUENCE [dbo].[SequenceOrganizationNumber] RESTART WITH ' + (SELECT CAST(@currentOrganizationNumber + @reservedNumber as nvarchar(10)))

                        exec sp_executesql @resetSQL;

                        -- Important: To make effect immediately
                        -- Current sequence value was occupied -> need to +1 for next value of Organization code.
                        SELECT NEXT VALUE FOR [dbo].SequenceOrganizationNumber

                        -- Current sequence value was occupied -> need to +1 for next value of Organization code.
                        SELECT @result = @currentOrganizationNumber + 1
                ";

            using (var command = Context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = sql;
                command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter
                {
                    ParameterName = "@reservedNumber",
                    Direction = System.Data.ParameterDirection.Input,
                    DbType = System.Data.DbType.Int32,
                    Value = quantityToReserve
                });
                var resultParam = new Microsoft.Data.SqlClient.SqlParameter()
                {
                    ParameterName = "@result",
                    Direction = System.Data.ParameterDirection.Output,
                    DbType = System.Data.DbType.Int32,
                    SqlDbType = System.Data.SqlDbType.Int
                };
                command.Parameters.Add(resultParam);

                try
                {
                    Context.Database.OpenConnection();
                    var result = command.ExecuteNonQuery();
                    return int.Parse(resultParam.Value.ToString());
                }
                finally
                {
                    command.Dispose();
                    Context.Database.CloseConnection();
                }
            }
        }
    }
}
