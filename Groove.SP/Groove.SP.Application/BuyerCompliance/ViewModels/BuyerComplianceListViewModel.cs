using System;
using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Application.Utilities;

namespace Groove.SP.Application.BuyerCompliance.ViewModels
{
    public class BuyerComplianceListViewModel : ViewModelBase<BuyerComplianceModel>
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string OrganizationName { get; set; }
        public BuyerComplianceStatus Status { get; set; }
        public BuyerComplianceStage Stage { get; set; }

        private string _statusName;
        public string StatusName
        {
            get
            {
                return string.IsNullOrEmpty(_statusName) ? EnumHelper<BuyerComplianceStatus>.GetDisplayName(this.Status) : _statusName;
            }
            set => _statusName = value;
        }

        private string _stageName;
        public string StageName
        {
            get
            {
                return string.IsNullOrEmpty(_stageName) ? EnumHelper<BuyerComplianceStage>.GetDisplayName(this.Stage) : _stageName;
            }
            set => _stageName = value;
        }
        
        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}
