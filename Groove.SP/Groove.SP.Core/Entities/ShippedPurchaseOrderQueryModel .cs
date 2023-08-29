using Groove.SP.Core.Models;
using System;

namespace Groove.SP.Core.Entities
{
    public class ShippedPurchaseOrderQueryModel
    {
        public long Id { get; set; }
        public string PONumber { get; set; }
        public DateTime? POIssueDate { get; set; }
        public PurchaseOrderStatus Status { get; set; }
        public string StatusName { get; set; }
        public POStageType Stage { get; set; }
        public string StageName { get; set; }
        public POType POType { get; set; }
        public DateTime? CargoReadyDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string Supplier { get; set; }
        public bool? IsProgressCargoReadyDates { get; set; }
        public int? ProgressNotifyDay { get; set; }
        public bool ProductionStarted { get; set; }
    }
}
