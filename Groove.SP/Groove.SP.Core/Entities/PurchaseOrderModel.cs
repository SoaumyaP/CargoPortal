using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;

namespace Groove.SP.Core.Entities
{
    public class PurchaseOrderModel : Entity
    {
        public long Id { get; set; }
        public string POKey { get; set; }
        public string PONumber { get; set; }
        public string CarrierCode { get; set; }
        public DateTime? EarliestDeliveryDate { get; set; }
        public DateTime? EarliestShipDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ExpectedShipDate { get; set; }
        public string GatewayCode { get; set; }        
        public string Incoterm { get; set; }
        public DateTime? LatestDeliveryDate { get; set; }
        public DateTime? LatestShipDate { get; set; }
        public string ModeOfTransport { get; set; }
        public long? NumberOfLineItems { get; set; }
        public DateTime? POIssueDate { get; set; }        
        public string CustomerReferences { get; set; }
        public string Department { get; set; }
        public string Season { get; set; }
        public string ShipFrom { get; set; }
        public long? ShipFromId { get; set; }
        public string ShipTo { get; set; }
        public long? ShipToId { get; set; }
        public string PaymentCurrencyCode { get; set; }
        public string PaymentTerms { get; set; }
        public PurchaseOrderStatus Status { get; set; }
        public POStageType Stage { get; set; }
        public POType POType { get; set; }
        public DateTime? CargoReadyDate { get; set; }
        public string PORemark { get; set; }
        public string POTerms { get; set; }
        public string HazardousMaterialsInstruction { get; set; }
        public string SpecialHandlingInstruction { get; set; }
        public string CarrierName { get; set; }
        public string GatewayName { get; set; }
        public string ShipFromName { get; set; }
        public string ShipToName { get; set; }
        public long? NotifyUserId { get; set; }
        public long? BlanketPOId { get; set; }
        public long? CruiseOrderId { get; set; }
        public EquipmentType? ContainerType { get; set; }
        public PurchaseOrderModel BlanketPO { get; set; }
        public bool ProductionStarted { set; get; }
        public bool QCRequired { set; get; }
        public bool ShortShip { set; get; }
        public bool SplitShipment { set; get; }
        public DateTime? ProposeDate { set; get; }
        public string Remark { set; get; }
        public decimal? Volume { get; set; }
        public decimal? GrossWeight { get; set; }
        public DateTime? ContractShipmentDate { get; set; }

        public virtual ICollection<PurchaseOrderContactModel> Contacts { get; set; }
        public virtual ICollection<POLineItemModel> LineItems { get; set; }
        public virtual ICollection<PurchaseOrderModel> AllocatedPOs { get; set; }

        protected override void AuditChildren(string user)
        {
            if (Contacts != null)
            {
                foreach (var contact in Contacts)
                {
                    contact.Audit(user);
                }
            }

            if (LineItems != null)
            {
                foreach (var lineItem in LineItems)
                {
                    lineItem.Audit(user);
                }
            }
        }
    }
}
