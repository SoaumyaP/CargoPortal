using Groove.SP.Core.Models;

namespace Groove.SP.Core.Entities
{
    public class BuyerApprovalQueryModel
    {
        public long Id { get; set; }        

        public string Reference { get; set; }

        public string Owner { get; set; }        

        public string Customer { get; set; }

        public BuyerApprovalStage Stage { get; set; }

        public string StageName { get; set; }

        public long POFulfillmentId { get; set; }

        public string POFulfillmentNumber { get; set; }

        public int POFulfillmentType { get; set; }

        public string POFulfillmentSupplier { get; set; }

    }
}