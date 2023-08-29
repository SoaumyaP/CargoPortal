using FluentValidation;
using System.Collections.Generic;
using Groove.SP.Application.Common;
using System.ComponentModel.DataAnnotations;
using Groove.SP.Core.Entities.Cruise;
using System;
using Groove.SP.Core.Models;
using Groove.SP.Application.PurchaseOrders.ViewModels;

namespace Groove.SP.Application.CruiseOrders.ViewModels
{
    public class CreateCruiseOrderViewModel : ViewModelBase<CruiseOrderModel>, IValidatableObject
    {
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

        public ICollection<CruiseOrderContactViewModel> Contacts { get; set; }
        public ICollection<CruiseOrderItemViewModel> Items { get; set; }

        public IEnumerable<ValidationResult> Validate(System.ComponentModel.DataAnnotations.ValidationContext validationContext)
        {
            var validator = (IValidator<CreateCruiseOrderViewModel>)validationContext.GetService(typeof(IValidator<CreateCruiseOrderViewModel>));
            var result = validator.Validate(this);
            foreach(var error in result.Errors)
            {
                yield return new ValidationResult(error.ErrorMessage, new[] { error.PropertyName });
            }        
        }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}
