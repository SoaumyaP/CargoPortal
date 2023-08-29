using Groove.SP.Application.BuyerApproval.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using System.Threading.Tasks;

namespace Groove.SP.Application.BuyerApproval.Services.Interfaces
{
    public interface IBuyerApprovalService : IServiceBase<BuyerApprovalModel, BuyerApprovalViewModel>
    {
        Task<BuyerApprovalViewModel> CreateAsync(BuyerApprovalViewModel viewModel);
        Task<BuyerApprovalViewModel> UpdateAsync(BuyerApprovalViewModel viewModel, long id);
        Task<DataSourceResult> ListAsync(DataSourceRequest request, bool isInternal, string userName, string affiliates, long? organizationId = 0, string userRole = "", string statisticKey = "");
        Task<BuyerApprovalViewModel> GetAsync(long id, bool isInternal, string userName, string affiliates);
        Task<BuyerApprovalViewModel> ApproveAsync(long id, BuyerApprovalViewModel viewModel, string userName);
        Task<BuyerApprovalViewModel> RejectAsync(long id, BuyerApprovalViewModel viewModel, string userName);
    }
}
