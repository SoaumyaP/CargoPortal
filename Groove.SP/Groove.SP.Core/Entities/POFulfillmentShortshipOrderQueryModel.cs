using System;

namespace Groove.SP.Core.Entities
{
    public class POFulfillmentShortshipOrderQueryModel
    {
        public long Id { get; set; }
        public long POFulfillmentId { get; set; }
        public string POFulfillmentNumber { get; set; }
        public long PurchaseOrderId { get; set; }
        public string CustomerPONumber { get; set; }
        public string ProductCode { get; set; }
        public int OrderedUnitQty { get; set; }
        public int FulfillmentUnitQty { get; set; }
        public int BalanceUnitQty { get; set; }
        public int? BookedPackage { get; set; }
        public decimal? Volume { get; set; }
        public decimal? GrossWeight { get; set; }
        public bool IsRead { get; set; }
        public DateTime ApprovedDate { get; set; }
    }
}
