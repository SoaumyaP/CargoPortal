using System;
using System.Collections.Generic;
using Groove.SP.Application.Common;
using Groove.SP.Application.EmailSetting.ViewModels;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;

namespace Groove.SP.Application.BuyerCompliance.ViewModels
{
    public class BuyerComplianceViewModel : ViewModelBase<BuyerComplianceModel>
    {
        public long Id { get; set; }

        public long OrganizationId { get; set; }

        public string OrganizationName { get; set; }

        public string PrincipleCode { get; set; }

        public string Name { get; set; }

        public DateTime EffectiveDate { get; set; }

        public bool EnforceCommercialInvoiceFormat { get; set; }

        public bool EnforcePackingListFormat { get; set; }

        public bool IsAssignedAgent { get; set; }

        public POType AllowToBookIn { get; set; }

        public bool IsAllowShowAdditionalInforPOListing { get; set; }

        public decimal? ShortShipTolerancePercentage { get; set; }

        public decimal? OvershipTolerancePercentage { get; set; }

        public BuyerComplianceStatus Status { get; set; }

        public string StatusName => EnumHelper<BuyerComplianceStatus>.GetDisplayName(this.Status);

        public BuyerComplianceStage Stage { get; set; }

        public string StageName => EnumHelper<BuyerComplianceStage>.GetDisplayName(Stage);

        public PurchaseOrderTransmissionMethodType PurchaseOrderTransmissionMethods { get; set; }

        public PurchaseOrderTransmissionFrequencyType PurchaseOrderTransmissionFrequency { get; set; }

        public ApprovalAlertFrequencyType ApprovalAlertFrequency { get; set; }

        public ApprovalDurationType ApprovalDuration { get; set; }

        public string PurchaseOrderTransmissionNotes { get; set; }

        public PurchaseOrderVerificationSettingViewModel PurchaseOrderVerificationSetting { get; set; }

        public ProductVerificationSettingViewModel ProductVerificationSetting { get; set; }

        public BookingTimelessViewModel BookingTimeless { get; set; }

        public ICollection<CargoLoadabilityViewModel> CargoLoadabilities { get; set; }

        public ICollection<EmailSettingViewModel> EmailSettings { get; set; }

        public ICollection<AgentAssignmentViewModel> AgentAssignments { get; set; }

        public ShippingComplianceViewModel ShippingCompliance { get; set; }

        public ComplianceSelectionViewModel ComplianceSelection { get; set; }

        public ICollection<BookingPolicyViewModel> BookingPolicies { get; set; }

        public ValidationResultType BookingPolicyAction { get; set; }

        public ApproverSettingType BookingApproverSetting { get; set; }

        public string BookingApproverUser { get; set; }

        public string BypassEmailDomain { get; set; }

        public IEnumerable<long> HSCodeShipFromCountryIds { get; set; }
        public IEnumerable<long> HSCodeShipFromIds { get; set; }

        public IEnumerable<long> HSCodeShipToCountryIds { get; set; }
        public IEnumerable<long> HSCodeShipToIds { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new System.NotImplementedException();
        }

        public bool IsProgressCargoReadyDates { get; set; }
        public bool IsCompulsory { get; set; }
        public int ProgressNotifyDay { get; set; }
        public bool IsEmailNotificationToSupplier { get; set; }
        public bool IsAllowMissingPO { get; set; }
        public string EmailNotificationTime { get; set; }
        public BuyerComplianceServiceType ServiceType { get; set; }
        public bool IntegrateWithWMS { get; set; }
        public AgentAssignmentMethodType AgentAssignmentMethod { get; set; }
    }
}