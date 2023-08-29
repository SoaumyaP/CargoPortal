using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.BuyerApproval
{
    public class ResetBuyerApprovalSequenceValueJob
    {
        private readonly ILogger<ResetBuyerApprovalSequenceValueJob> _logger;
        private readonly IBuyerApprovalRepository _buyerApprovalRepository;

        public ResetBuyerApprovalSequenceValueJob(ILogger<ResetBuyerApprovalSequenceValueJob> logger,
            IRepository<BuyerApprovalModel> buyerApprovalRepository)
        {
            _logger = logger;
            _buyerApprovalRepository = (IBuyerApprovalRepository)buyerApprovalRepository;
        }

        public async Task ExecuteAsync()
        {
            _logger.LogInformation("ResetBuyerApprovalSequenceValue Job Starting...");
            await _buyerApprovalRepository.ResetBuyerApprovalSequenceValueAsync();
            _logger.LogInformation("ResetBuyerApprovalSequenceValue Job Completed.");
        }
    }
}
