using Groove.SP.Core.Models;
using System;
using System.ComponentModel;

namespace Groove.SP.Application.PurchaseOrders.ViewModels
{
    /// <summary>
    /// Model to import Purchase Order via Excel file (PO Header tab).
    /// </summary>
    /// <remarks>
    /// <b>Property order must be the same as the Excel file</b>
    /// </remarks>
    public class ExcelPOViewModel
    {
        public string PONumber { get; set; }
        public DateTime? POIssueDate { get; set; }
        public DateTime? CargoReadyDate { get; set; }
        public IncotermType? Incoterm { get; set; }
        public long? NumberOfLineItems { get; set; }
        public string CustomerReferences { get; set; }
        public string Department { get; set; }
        public string Season { get; set; }
        public string PaymentTerms { get; set; }
        public ModeOfTransportType? ModeOfTransport { get; set; }
        public string CarrierCode { get; set; }
        public string GatewayCode { get; set; }
        public string ShipFrom { get; set; }
        public string ShipTo { get; set; }
        public string PaymentCurrencyCode { get; set; }
        public DateTime? EarliestDeliveryDate { get; set; }
        public DateTime? EarliestShipDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ExpectedShipDate { get; set; }
        public DateTime? LatestDeliveryDate { get; set; }
        public DateTime? LatestShipDate { get; set; }
        public string PORemark { get; set; }
        public string POTerms { get; set; }
        public string HazardousMaterialsInstruction { get; set; }
        public string SpecialHandlingInstruction { get; set; }
        public string CarrierName { get; set; }
        public string GatewayName { get; set; }
        public string ShipFromName { get; set; }
        public string ShipToName { get; set; }
        public EquipmentType? ContainerType { get; set; }
        public PurchaseOrderStatus? Status { get; set; }
        public decimal? Volume { get; set; }
        public decimal? GrossWeight { get; set; }
        public DateTime? ContractShipmentDate { get; set; }
        public string Row { get; set; }
        public bool ProductionStarted { set; get; }
        public bool QCRequired { set; get; }
        public bool ShortShip { set; get; }
        public bool SplitShipment { set; get; }
        public DateTime? ProposeDate { set; get; }
        public string Remark { set; get; }
        
    }
}
