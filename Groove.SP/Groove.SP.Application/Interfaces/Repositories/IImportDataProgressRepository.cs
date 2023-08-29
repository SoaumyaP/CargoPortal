using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using System.Threading.Tasks;

namespace Groove.SP.Application.Interfaces.Repositories
{
    public interface IImportDataProgressRepository : IRepository<ImportDataProgressModel>
    {
        Task UpdateStatusAsync(long id, ImportDataProgressStatus status, string result, string log = null);

        Task UpdateProgressAsync(long id, int completedSteps, int? totalSteps = null);
    }
}
