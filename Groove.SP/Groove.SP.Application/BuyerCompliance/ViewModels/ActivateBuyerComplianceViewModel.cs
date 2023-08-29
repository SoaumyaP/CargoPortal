using Groove.SP.Application.BuyerCompliance.Validations;
using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using FluentValidation;
using System.Collections.Generic;
using System.Linq;

namespace Groove.SP.Application.BuyerCompliance.ViewModels
{
    public class ActivateBuyerComplianceViewModel : ViewModelBase<BuyerComplianceModel>
    {
        public ActivateBuyerComplianceViewModel()
        {
            BookingPolicies = new List<BookingPolicyViewModel>();
            CargoLoadabilities = new List<CargoLoadabilityViewModel>();
            AgentAssignments = new List<AgentAssignmentViewModel>();
        }

        public long Id { get; set; }

        public long? OrganizationId { get; set; }

        public string OrganizationName { get; set; }

        public string PrincipleCode { get; set; }

        public string Name { get; set; }

        public bool EnforceCommercialInvoiceFormat { get; set; }

        public bool EnforcePackingListFormat { get; set; }

        public decimal? ShortShipTolerancePercentage { get; set; }

        public decimal? OvershipTolerancePercentage { get; set; }

        public PurchaseOrderTransmissionFrequencyType PurchaseOrderTransmissionFrequency { get; set; }

        public ApprovalAlertFrequencyType ApprovalAlertFrequency { get; set; }

        public ApprovalDurationType ApprovalDuration { get; set; }

        public PurchaseOrderVerificationSettingViewModel PurchaseOrderVerificationSetting { get; set; }

        public ProductVerificationSettingViewModel ProductVerificationSetting { get; set; }

        public BookingTimelessViewModel BookingTimeless { get; set; }

        public ICollection<CargoLoadabilityViewModel> CargoLoadabilities { get; set; }

        public ICollection<AgentAssignmentViewModel> AgentAssignments { get; set; }
        public ICollection<AgentAssignmentViewModel> AirAgentAssignments { get; set; }

        public ShippingComplianceViewModel ShippingCompliance { get; set; }

        public ComplianceSelectionViewModel ComplianceSelection { get; set; }

        public ICollection<BookingPolicyViewModel> BookingPolicies { get; set; }

        public BuyerComplianceStatus Status { get; set; }

        public string PurchaseOrderTransmissionNotes { get; set; }

        public BuyerComplianceStage Stage { get; set; }

        public ValidationResultType BookingPolicyAction { get; set; }

        public ApproverSettingType BookingApproverSetting { get; set; }

        public string BookingApproverUser { get; set; }

        public PurchaseOrderTransmissionMethodType PurchaseOrderTransmissionMethods { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new ActivateBuyerComplianceValidator().ValidateAndThrow(this);
        }

        protected override void AuditChildren(string user)
        {
            PurchaseOrderVerificationSetting.Audit(user);
            ProductVerificationSetting.Audit(user);
            BookingTimeless.Audit(user);
            ShippingCompliance.Audit(user);
            ComplianceSelection.Audit(user);

            if (CargoLoadabilities != null)
            {
                foreach (var item in CargoLoadabilities)
                {
                    item.Audit(user);
                }
            }

            if (BookingPolicies != null)
            {
                foreach (var item in BookingPolicies)
                {
                    item.Audit(user);
                }
            }

            if (AgentAssignments != null)
            {
                foreach (var item in AgentAssignments)
                {
                    item.Audit(user);
                }
            }

            if (AirAgentAssignments != null)
            {
                foreach (var item in AirAgentAssignments)
                {
                    item.Audit(user);
                }
            }
        }

        public void MergeAgentAssignment()
        {
            if (AgentAssignments?.Count > 0 && AirAgentAssignments?.Count > 0)
            {
                AgentAssignments = AgentAssignments.Concat(AirAgentAssignments).ToList();
            }
        }
    }
}
