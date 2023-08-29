using System;
using System.Collections.Generic;
using Groove.SP.Core.Models;

namespace Groove.SP.Core.Entities
{
    public class BuyerComplianceModel : Entity
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

        public bool IsAllowMissingPO { get; set; }
        public bool IsAllowShowAdditionalInforPOListing { get; set; }

        public decimal? ShortShipTolerancePercentage { get; set; }

        public decimal? OvershipTolerancePercentage { get; set; }

        public BuyerComplianceStatus Status { get; set; }

        public BuyerComplianceStage Stage { get; set; }

        public PurchaseOrderTransmissionMethodType PurchaseOrderTransmissionMethods { get; set; }

        public PurchaseOrderTransmissionFrequencyType PurchaseOrderTransmissionFrequency { get; set; }

        public ApprovalAlertFrequencyType ApprovalAlertFrequency { get; set; }

        public ApprovalDurationType ApprovalDuration { get; set; }

        public string PurchaseOrderTransmissionNotes { get; set; }

        public PurchaseOrderVerificationSettingModel PurchaseOrderVerificationSetting { get; set; }

        public ProductVerificationSettingModel ProductVerificationSetting { get; set; }

        public BookingTimelessModel BookingTimeless { get; set; }

        public virtual ICollection<AgentAssignmentModel> AgentAssignments { get; set; }

        public virtual ICollection<CargoLoadabilityModel> CargoLoadabilities { get; set; }

        public virtual ICollection<EmailSettingModel> EmailSettings { get; set; }

        public ShippingComplianceModel ShippingCompliance { get; set; }

        public ComplianceSelectionModel ComplianceSelection { get; set; }

        public virtual ICollection<BookingPolicyModel> BookingPolicies { get; set; }

        public virtual ICollection<BookingValidationLogModel> BookingValidationLogs { get; set; }

        public ValidationResultType BookingPolicyAction { get; set; }

        public ApproverSettingType BookingApproverSetting { get; set; }

        public string BookingApproverUser { get; set; }

        public string BypassEmailDomain { get; set; }

        public string HSCodeShipFromCountryIds { get; set; }

        public string HSCodeShipToCountryIds { get; set; }

        public bool IsProgressCargoReadyDate { get; set; }
        public bool IsCompulsory { get; set; }
        public bool IsEmailNotificationToSupplier { get; set; }
        public string EmailNotificationTime { get; set; }
        public int ProgressNotifyDay { get; set; }

        public BuyerComplianceServiceType ServiceType { get; set; }
        public bool IntegrateWithWMS { get; set; }
        public AgentAssignmentMethodType AgentAssignmentMethod { get; set; }
    }
}