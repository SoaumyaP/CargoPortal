using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using System;
using Groove.SP.Core.Models;
using Groove.SP.Application.Utilities;
using Groove.SP.Application.POFulfillment.ViewModels;

namespace Groove.SP.Application.BuyerApproval.ViewModels
{
    public class BuyerApprovalViewModel : ViewModelBase<BuyerApprovalModel>
    {
        public long Id { get; set; }

        public BuyerApprovalStatus Status { get; set; }

        public string StageName => EnumHelper<BuyerApprovalStage>.GetDisplayName(Stage);

        public BuyerApprovalStage Stage { get; set; }

        public long? POFulfillmentId { get; set; }

        public string Reference { get; set; }

        public string Transaction { get; set; }

        public string Owner { get; set; }

        public string ExceptionTypeName => EnumHelper<ExceptionType>.GetDisplayName(ExceptionType);

        public ExceptionType ExceptionType { get; set; }

        public DateTime? DueOnDate { get; set; }

        public string Customer { get; set; }

        public ApproverSettingType ApproverSetting { get; set; }

        public string ApproverUser { get; set; }

        public long? ApproverOrgId { get; set; }

        public DateTime? ResponseOn { get; set; }

        public string RequestByOrganization { get; set; }

        public string Requestor { get; set; }

        public string ExceptionActivity { get; set; }

        public DateTime ActivityDate { get; set; }

        public ApprovalAlertFrequencyType AlertNotificationFrequencyType { get; set; }

        public string Severity { get; set; }

        public string ExceptionDetail { get; set; }

        public virtual SummaryBuyerApprovalPOFFViewModel POFulfillment { get; set; }

        public string Reason { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}
