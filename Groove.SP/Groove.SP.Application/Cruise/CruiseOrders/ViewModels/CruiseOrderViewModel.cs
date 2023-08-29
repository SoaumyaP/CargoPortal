using FluentValidation;
using System.Collections.Generic;
using Groove.SP.Application.Common;
using System.ComponentModel.DataAnnotations;
using Groove.SP.Core.Entities.Cruise;
using System;
using Groove.SP.Core.Models;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using Groove.SP.Application.Utilities;
using System.Linq;
using Groove.SP.Application.Shipments.ViewModels;
using Groove.SP.Application.Cruise.CruiseOrderWarehouseInfos.ViewModels;

namespace Groove.SP.Application.CruiseOrders.ViewModels
{
    public class CruiseOrderViewModel : ViewModelBase<CruiseOrderModel>, IValidatableObject
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
        public CruiseOrderStatus? POStatus { get; set; }
        public string POSubject { get; set; }
        public DateTime? PODate { get; set; }

        
        private string _statusName;
        public string StatusName
        {
            get
            {
                return string.IsNullOrEmpty(_statusName) ? EnumHelper<CruiseOrderStatus>.GetDisplayName(this.POStatus.Value) : _statusName;
            }
            set => _statusName = value;
        }

        public string Customer => Contacts?.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Principal)?.CompanyName ?? string.Empty;
        public string Supplier => Contacts?.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Supplier)?.CompanyName ?? string.Empty;
        public string Consignee => Contacts?.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Consignee)?.CompanyName ?? string.Empty;

        public IEnumerable<CruiseOrderContactViewModel> Contacts { get; set; }
        public IEnumerable<CruiseOrderItemViewModel> Items { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            throw new NotImplementedException();
        }
    }
}
