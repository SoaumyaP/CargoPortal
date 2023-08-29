using System;
using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Application.Utilities;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace Groove.SP.Application.PurchaseOrders.ViewModels
{
    public class PurchaseOrderListViewModel : ViewModelBase<PurchaseOrderModel>
    {
        public long Id { get; set; }
        public string PONumber { get; set; }
        public string CustomerReferences { set; get; }
        public DateTime? POIssueDate { get; set; }
        public PurchaseOrderStatus Status { get; set; }

        private string _statusName;
        public string StatusName
        {
            get
            {
                return string.IsNullOrEmpty(_statusName) ? EnumHelper<PurchaseOrderStatus>.GetDisplayName(this.Status) : _statusName;
            }
            set => _statusName = value;
        }
        public POStageType Stage { get; set; }

        private string _stageName;
        public string StageName
        {
            get
            {
                return string.IsNullOrEmpty(_stageName) ? EnumHelper<POStageType>.GetDisplayName(this.Stage) : _stageName;
            }
            set => _stageName = value;
        }

        public string ContainerTypeName { set; get; }

        public POType POType { get; set; }

        public DateTime? CargoReadyDate { get; set; }

        public string Supplier { get; set; }
        public bool? IsProgressCargoReadyDates { get; set; }
        public int? ProgressNotifyDay { get; set; }
        public bool ProductionStarted { get; set; }


        public string ModeOfTransport { set; get; }
        public string ShipFrom { set; get; }
        public string ShipTo { set; get; }
        public string Incoterm { set; get; }
        public DateTime? ExpectedDeliveryDate { set; get; }
        public DateTime? ExpectedShipDate { set; get; }
        public EquipmentType ContainerType { set; get; }
        public string PORemark { set; get; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}
