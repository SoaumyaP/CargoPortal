namespace Groove.SP.Core.Entities
{
    public class GlobalIdApprovalModel : Entity
    {
        public string GlobalId { get; set; }

        public long ApprovalId { get; set; }

        public GlobalIdModel ReferenceEntity { get; set; }

        public BuyerApprovalModel Approval { get; set; }
    }
}
