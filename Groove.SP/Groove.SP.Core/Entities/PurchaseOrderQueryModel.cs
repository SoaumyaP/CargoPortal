using Groove.SP.Core.Models;
using System;

namespace Groove.SP.Core.Entities
{
    public class PurchaseOrderQueryModel
    {
        public long Id { get; set; }
        public string PONumber { get; set; }
        public string CustomerReferences { set; get; }
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

        public string  ModeOfTransport { set; get; }
        public string ShipFrom { set; get; }
        public string ShipTo { set; get; }
        public string Incoterm { set; get; }
        public DateTime? ExpectedDeliveryDate { set; get; }
        public DateTime? ExpectedShipDate { set; get; }
        public EquipmentType? ContainerType { set; get; }
        public string PORemark { set; get; }
    }

    public class PurchaseOrderSingleQueryModel: DataSourceSingleQueryModel
    {
        public long Id { get; set; }
        public string PONumber { get; set; }
        public string CustomerReferences { set; get; }
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

        public string ModeOfTransport { set; get; }
        public string ShipFrom { set; get; }
        public string ShipTo { set; get; }
        public string Incoterm { set; get; }
        public DateTime? ExpectedDeliveryDate { set; get; }
        public DateTime? ExpectedShipDate { set; get; }
        public EquipmentType? ContainerType { set; get; }
        public string PORemark { set; get; }
    }
}
