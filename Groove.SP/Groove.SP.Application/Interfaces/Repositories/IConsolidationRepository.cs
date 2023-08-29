using Groove.SP.Core.Entities;
using System.Threading.Tasks;

namespace Groove.SP.Application.Interfaces.Repositories
{
    public interface IConsolidationRepository : IRepository<ConsolidationModel>
    {
        Task<string> GetNextLoadPlanIDSequenceValueAsync();
        Task ResetLoadPlanIDSequenceValueAsync();
    }
}
