using System;
using System.Collections.Generic;
using System.Linq;
using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Application.Utilities;
using Groove.SP.Application.PurchaseOrderContact.ViewModels;
using Groove.SP.Application.POFulfillment.ViewModels;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Groove.SP.Application.Shipments.ViewModels;
using Groove.SP.Application.ViewSetting.ViewModels;
using Groove.SP.Application.ViewSetting.Interfaces;

namespace Groove.SP.Application.PurchaseOrders.ViewModels
{
    public class PurchaseOrderViewModel : ViewModelBase<PurchaseOrderModel>, IHasViewSetting
    {
        public long Id { get; set; }
        public string POKey { get; set; }
        public string PONumber { get; set; }
        public string ModeOfTransport { get; set; }
        public long? NumberOfLineItems { get; set; }
        public DateTime? POIssueDate { get; set; }
        public string ShipFrom { get; set; }
        public long? ShipFromId { get; set; }
        public string ShipTo { get; set; }
        public long? ShipToId { get; set; }
        public string Incoterm { get; set; }
        public string CarrierCode { get; set; }
        public string GatewayCode { get; set; }
        public string PaymentCurrencyCode { get; set; }
        public DateTime? EarliestDeliveryDate { get; set; }
        public DateTime? EarliestShipDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ExpectedShipDate { get; set; }
        public DateTime? LatestDeliveryDate { get; set; }
        public DateTime? LatestShipDate { get; set; }
        public string CustomerReferences { get; set; }
        public string Department { get; set; }
        public string Season { get; set; }
        public string PORemark { get; set; }
        public string POTerms { get; set; }
        public string HazardousMaterialsInstruction { get; set; }
        public string SpecialHandlingInstruction { get; set; }
        public string CarrierName { get; set; }
        public string GatewayName { get; set; }
        public string ShipFromName { get; set; }
        public string ShipToName { get; set; }
        public long? BlanketPOId { get; set; }
        public PurchaseOrderStatus Status { get; set; }
        public PurchaseOrderViewModel BlanketPO { get; set; }
        public bool IsProgressCargoReadyDates { get; set; }
        public bool IsCompulsory { get; set; }

        /// <summary>
        /// From dbo.BuyerCompliances.
        /// </summary>
        public bool IsAllowMissingPO { get; set; }

        public int ProgressNotifyDay { get; set; }
        public bool ProductionStarted { set; get; }
        public bool QCRequired { set; get; }
        public bool ShortShip { set; get; }
        public bool SplitShipment { set; get; }
        public DateTime? ProposeDate { set; get; }
        public string Remark { set; get; }

        public decimal? Volume { get; set; }
        public decimal? GrossWeight { get; set; }
        public DateTime? ContractShipmentDate { get; set; }


        [JsonConverter(typeof(StringEnumConverter))]
        public EquipmentType? ContainerType { get; set; }
        public POType POType { get; set; }
        public string ContainerTypeName => ContainerType.HasValue ? EnumHelper<EquipmentType>.GetDisplayName(ContainerType.Value) : string.Empty;

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
        public DateTime? CargoReadyDate { get; set; }

        // From Compliance settings
        public POType AllowToBookIn { get; set; }
        public BuyerComplianceServiceType CustomerServiceType { get; set; }

        public string Supplier { get; set; }
        public string Customer { get; set; }
        public string Shipper { get; set; }
        public string Consignee { get; set; }

        public ICollection<PurchaseOrderContactViewModel> Contacts { get; set; }
        public ICollection<POLineItemViewModel> LineItems { get; set; }
        public ICollection<SummaryPOFulfillmentViewModel> Fulfillments { get; set; }
        public ICollection<ViewAllocatedPOViewModel> AllocatedPOs { get; set; }
        public ICollection<ShipmentViewModel> Shipments { get; set; }

        [JsonIgnore]
        public string ViewSettingModuleId { get; set; } = Core.Models.ViewSettingModuleId.PO_DETAIL;

        public IEnumerable<ViewSettingDataSourceViewModel> ViewSettings { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}