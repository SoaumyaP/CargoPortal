using Groove.SP.Core.Entities;
using System.Threading.Tasks;

namespace Groove.SP.Application.Interfaces.Repositories
{
    public interface IBuyerApprovalRepository : IRepository<BuyerApprovalModel>
    {
        Task<string> GetNextBuyerApprovalSequenceValueAsync();
        Task ResetBuyerApprovalSequenceValueAsync();
    }
}