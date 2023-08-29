using System;
using System.Collections.Generic;
using Groove.SP.Core.Models;

namespace Groove.SP.Core.Entities
{
    public class BuyerApprovalModel : Entity
    {
        public long Id { get; set; }

        public BuyerApprovalStatus Status { get; set; }

        public BuyerApprovalStage Stage { get; set; }

        public long POFulfillmentId { get; set; }

        public string Transaction { get; set; }

        public string Reference { get; set; }

        public string Owner { get; set; }

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

        public string Reason { get; set; }

        public virtual POFulfillmentModel POFulfillment { get; set; }

        public virtual ICollection<GlobalIdApprovalModel> GlobalIdApprovals { get; set; }
    }
}