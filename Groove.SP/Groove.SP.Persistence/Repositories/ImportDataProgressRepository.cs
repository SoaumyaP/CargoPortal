using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Persistence.Contexts;
using Groove.SP.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace Groove.SP.Persistence.Repositories
{
    public class ImportDataProgressRepository : Repository<SpContext, ImportDataProgressModel>, IImportDataProgressRepository
    {
        public ImportDataProgressRepository(SpContext context)
            : base(context)
        { }

        public async Task UpdateProgressAsync(long id, int completedSteps, int? totalSteps = null)
        {
            await this.Context.Database.ExecuteSqlRawAsync(
                "spu_UpdateImportDataProgress @id, @completedSteps, @totalSteps",
                new SqlParameter("@id", id),
                new SqlParameter("@completedSteps", completedSteps),
                new SqlParameter("@totalSteps", (object)totalSteps ?? DBNull.Value));
        }

        public async Task UpdateStatusAsync(long id, ImportDataProgressStatus status, string result, string log = null)
        {
            await this.Context.Database.ExecuteSqlRawAsync(
                "spu_UpdateImportDataProgressStatus @id, @status, @result, @log",
                new SqlParameter("@id", id),
                new SqlParameter("@status", status),
                new SqlParameter("@result", result ?? string.Empty),
                new SqlParameter("@log", log ?? string.Empty));
        }
    }
}
