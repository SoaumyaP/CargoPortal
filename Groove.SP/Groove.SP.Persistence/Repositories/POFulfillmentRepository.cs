using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities;
using Groove.SP.Persistence.Contexts;
using Groove.SP.Persistence.Repositories.Base;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace Groove.SP.Persistence.Repositories
{
    public class POFulfillmentRepository : Repository<SpContext, POFulfillmentModel>, IPOFulfillmentRepository
    {
        protected const int DEFAULT_SEQUENCE_START_WITH = 1;
        protected const int DEFAULT_SEQUENCE_INCREMENT_BY = 1;
        public POFulfillmentRepository(SpContext context) : base(context)
        {
        }

        public async Task<string> GetNextPOFFSequenceValueAsync()
        {
            using (var command = Context.Database.GetDbConnection().CreateCommand())
            {
                command.Transaction = Context.Database.CurrentTransaction?.GetDbTransaction();
                command.CommandText = "SELECT NEXT VALUE FOR [dbo].SequencePOFFNumber";
                Context.Database.OpenConnection();
                var nextValue = (short)await command.ExecuteScalarAsync();

                return $"{nextValue:0000}";
            }
        }

        public async Task<string> GetNextPOFFSequenceValueAsync(long customerOrgId)
        {
            using (var command = Context.Database.GetDbConnection().CreateCommand())
            {
                var nextValue = DEFAULT_SEQUENCE_START_WITH;
                var sequenceName = $"ORG#{customerOrgId}_SequencePOFFNumber";

                command.Transaction = Context.Database.CurrentTransaction?.GetDbTransaction();
                command.CommandText = $"SELECT NEXT VALUE FOR [dbo].{sequenceName}";

                Context.Database.OpenConnection();
                try
                {
                    nextValue = (short)await command.ExecuteScalarAsync();
                }
                catch (SqlException ex)
                {
                    // Create a new one if it doesn't already exist...

                    if (ex.Message != null && ex.Message.StartsWith("Invalid object name"))
                    {
                        command.CommandText = $@"CREATE SEQUENCE [dbo].{sequenceName} AS SMALLINT
                                                START WITH {DEFAULT_SEQUENCE_START_WITH}
                                                INCREMENT BY {DEFAULT_SEQUENCE_INCREMENT_BY}";
                        await command.ExecuteNonQueryAsync();

                        command.CommandText = $"SELECT NEXT VALUE FOR {sequenceName}";
                        nextValue = (short)await command.ExecuteScalarAsync();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }

                return $"{nextValue:0000}";
            }
        }

        public async Task<string> GetNextPOFFLoadSequenceValueAsync()
        {
            using (var command = Context.Database.GetDbConnection().CreateCommand())
            {
                command.Transaction = Context.Database.CurrentTransaction?.GetDbTransaction();
                command.CommandText = "SELECT NEXT VALUE FOR [dbo].SequencePOFFLoadNumber";
                Context.Database.OpenConnection();
                var nextValue = (int)await command.ExecuteScalarAsync();

                return $"{nextValue:00000}";
            }
        }

        public async Task ResetPOFFSequenceValueAsync()
        {
            var allSequenceNames = await ListAllPOFFSequenceNameAsync();
            foreach (var sequenceName in allSequenceNames)
            {
                await Context.Database.ExecuteSqlRawAsync($"ALTER SEQUENCE dbo.{sequenceName} RESTART WITH {DEFAULT_SEQUENCE_START_WITH}");
            }
        }

        private async Task<List<string>> ListAllPOFFSequenceNameAsync()
        {
            using (var command = Context.Database.GetDbConnection().CreateCommand())
            {
                command.Transaction = Context.Database.CurrentTransaction?.GetDbTransaction();
                command.CommandText = @"SELECT name
                                        FROM sys.sequences
                                        WHERE name like '%_SequencePOFFNumber'";

                Context.Database.OpenConnection();

                var reader = await command.ExecuteReaderAsync();
                Func<DbDataReader, List<string>> mapping = (reader) =>
                {
                    var mappedData = new List<string>();

                    while (reader.Read())
                    {
                        string newRow = reader[0].ToString();
                        mappedData.Add(newRow);
                    }

                    return mappedData;
                };

                var result = mapping(reader);
                reader.Close();

                return result;
            }
        }

        public async Task ResetPOFFLoadSequenceValueAsync()
        {
            await Context.Database.ExecuteSqlRawAsync("ALTER SEQUENCE dbo.SequencePOFFLoadNumber RESTART WITH 1");
        }
    }
}