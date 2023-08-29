using System;

namespace Groove.SP.Core.Entities
{
    public class BookingValidationLogModel : Entity
    {
        public long Id { get; set; }

        public long POFulfillmentId { get; set; }
        public string POFulfillmentNumber { get; set; }
        // Version of PO Fulfillment
        public string Version { get; set; }
        public string Customer { get; set; }
        public string Supplier { get; set; }
        public string SubmittedBy { get; set; }
        public DateTime SubmissionDate { get; set; }
        public long PolicyID { get; set; }
        public DateTime ActionDatetime { get; set; }
        public string ActionType { get; set; }
        public string ActionActivity { get; set; }
        public string ActivityRemark { get; set; }
        public string FulfillmentJSON { get; set; }
        public string PolicyJSON { get; set; }

        public long BuyerComplianceId { get; set; }
        public BuyerComplianceModel BuyerCompliance { get; set; }
    }
}
