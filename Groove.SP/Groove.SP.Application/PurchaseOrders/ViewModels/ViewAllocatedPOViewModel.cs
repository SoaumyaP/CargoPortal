using Groove.SP.Application.Common;
using Groove.SP.Application.Utilities;
using Groove.SP.Application.ViewSetting.Interfaces;
using Groove.SP.Application.ViewSetting.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Groove.SP.Application.PurchaseOrders.ViewModels
{
    public class ViewAllocatedPOViewModel : ViewModelBase<PurchaseOrderModel>, IHasViewSetting
    {
        public long Id { get; set; }

        public string PONumber { get; set; }

        public string ShipTo { get; set; }

        public long? ShipToId { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }

        public string ShipToName { get; set; }

        public long? BlanketPOId { get; set; }

        public PurchaseOrderStatus Status { get; set; }

        public POStageType Stage { get; set; }

        private string _statusName;
        public string StatusName
        {
            get
            {
                return string.IsNullOrEmpty(_statusName) ? EnumHelper<PurchaseOrderStatus>.GetDisplayName(this.Status) : _statusName;
            }
            set => _statusName = value;
        }

        private string _stageName;
        public string StageName
        {
            get
            {
                return string.IsNullOrEmpty(_stageName) ? EnumHelper<POStageType>.GetDisplayName(this.Stage) : _stageName;
            }
            set => _stageName = value;
        }

        [JsonIgnore]
        public string ViewSettingModuleId { get; set; } = Core.Models.ViewSettingModuleId.PO_DETAIL_ALLOCATEDPOS;

        public IEnumerable<ViewSettingDataSourceViewModel> ViewSettings { get; set; }

        public ICollection<POLineItemViewModel> LineItems { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}