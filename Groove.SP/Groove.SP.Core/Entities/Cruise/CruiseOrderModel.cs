using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Groove.SP.Core.Entities.Cruise
{
    public class CruiseOrderModel : Entity
    {
        public long Id { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public DateTime? ActualShipDate { get; set; }
        public string ApprovalStatus { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string Approver { get; set; }
        public string BudgetAccount { get; set; }
        public string BudgetId { get; set; }
        public int? BudgetPeriod { get; set; }
        public int? BudgetYear { get; set; }
        public string CertificateId { get; set; }
        public string CertificateNumber { get; set; }
        public string CreationUser { get; set; }
        public string Delivered { get; set; }
        public string DeliveryMeans { get; set; }
        public string Department { get; set; }
        public string FirstReceivingPoint { get; set; }
        public decimal? Invoiced { get; set; }
        public string MaintenanceObject { get; set; }
        public string Maker { get; set; }
        public string POCause { get; set; }
        public string POId { get; set; }
        public string POType { get; set; }
        public string POPriority { get; set; }
        public DateTime? RequestApprovedDate { get; set; }
        public DateTime? RequestDate { get; set; }
        public string RequestPriority { get; set; }
        public string RequestType { get; set; }
        public string RequestType2 { get; set; }
        public string RequestType3 { get; set; }
        public string Requestor { get; set; }
        public string Ship { get; set; }
        public string WithWO { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public string PONumber { get; set; }
        public CruiseOrderStatus POStatus { get; set; }
        public string POSubject { get; set; }
        public DateTime? PODate { get; set; }       

        public virtual ICollection<CruiseOrderContactModel> Contacts { get; set; }
        public virtual ICollection<CruiseOrderItemModel> Items { get; set; }

        protected override void AuditChildren(string user = AppConstant.SYSTEM_USERNAME)
        {
           
            if (Contacts != null && Contacts.Any())
            {
                foreach (var contact in Contacts)
                {
                    var utcNow = DateTime.UtcNow;
                    if (string.IsNullOrWhiteSpace(contact.CreatedBy))
                    {
                        contact.CreatedBy = user;
                        contact.CreatedDate = utcNow;
                    }

                    contact.UpdatedBy = user;
                    contact.UpdatedDate = utcNow;
                }
            }

            if (Items != null && Items.Any())
            {                
                foreach (var item in Items)
                {
                    var utcNow = DateTime.UtcNow;
                    if (string.IsNullOrWhiteSpace(item.CreatedBy))
                    {
                        item.CreatedBy = user;
                        item.CreatedDate = utcNow;
                    }

                    item.UpdatedBy = user;
                    item.UpdatedDate = utcNow;
                }
            }
        }
    }
}
