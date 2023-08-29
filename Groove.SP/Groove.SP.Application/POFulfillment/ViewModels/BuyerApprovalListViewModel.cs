using Groove.SP.Core.Models;

namespace Groove.SP.Application.BuyerApproval.ViewModels
{
    public class BuyerApprovalListViewModel
    {
        public long Id { get; set; }

        public BuyerApprovalStatus Status { get; set; }
        
        public BuyerApprovalStage Stage { get; set; }
    }
}
