using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Groove.SP.Core.Models;
using Groove.SP.Application.Utilities;
using Groove.SP.Application.Attachment.ViewModels;
using Groove.SP.Application.Shipments.ViewModels;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Application.ViewSetting.Interfaces;
using Groove.SP.Application.ViewSetting.ViewModels;

namespace Groove.SP.Application.BulkFulfillment.ViewModels
{
    public class BulkFulfillmentViewModel : ViewModelBase<POFulfillmentModel>, IHasViewSetting
    {
        public long Id { get; set; }

        public string Number { get; set; }

        public string Owner { get; set; }

        public POFulfillmentStatus Status { get; set; }

        public string StatusName => EnumHelper<POFulfillmentStatus>.GetDisplayName(Status);

        public POFulfillmentStage Stage { get; set; }

        public string StageName => EnumHelper<POFulfillmentStage>.GetDisplayName(Stage);

        public DateTime? CargoReadyDate { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public IncotermType Incoterm { get; set; }

        public bool IsGeneratePlanToShip { get; set; }

        public bool IsPartialShipment { get; set; }

        public bool IsContainDangerousGoods { get; set; }

        public bool IsAllowMixedCarton { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ModeOfTransportType ModeOfTransport { get; set; }

        public long PreferredCarrier { get; set; }

        public string LogisticsServiceName => EnumHelper<LogisticsServiceType>.GetDisplayName(LogisticsService);

        public string MovementTypeName => EnumHelper<MovementType>.GetDisplayName(MovementType);

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public LogisticsServiceType LogisticsService { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public MovementType MovementType { get; set; }

        // LocationId
        public long ShipFrom { get; set; }

        // LocationId
        public long ShipTo { get; set; }

        // LocationDescription
        public string ShipFromName { get; set; }

        // LocationDescription
        public string ShipToName { get; set; }

        public DateTime? ExpectedShipDate { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }

        public string Remarks { get; set; }

        public string VesselName { get; set; }

        public string VoyageNo { get; set; }

        public bool IsRejected { get; set; }

        public bool IsForwarderBookingItineraryReady { get; set; }

        public bool IsNeedToPlanToShipAgain { get; set; }

        public bool IsFulfilledFromPO { get; set; }

        public POType? FulfilledFromPOType { get; set; }

        public bool IsShipperPickup { get; set; }

        public bool IsNotifyPartyAsConsignee { get; set; }

        public bool IsCIQOrFumigation { get; set; }

        public bool IsBatteryOrChemical { get; set; }

        public bool IsExportLicence { get; set; }

        // LocationId
        public long? ReceiptPortId { get; set; }

        // LocationId
        public long? DeliveryPortId { get; set; }

        // LocationDescription
        public string ReceiptPort { get; set; }

        // LocationDescription
        public string DeliveryPort { get; set; }

        public string CYEmptyPickupTerminalCode { get; set; }

        public string CYEmptyPickupTerminalDescription { get; set; }

        public string CFSWarehouseCode { get; set; }

        public string CFSWarehouseDescription { get; set; }

        public DateTime? CYClosingDate { get; set; }

        public DateTime? CFSClosingDate { get; set; }


        public ICollection<AttachmentViewModel> Attachments { get; set; }

        public ICollection<BulkFulfillmentContactViewModel> Contacts { get; set; }

        public ICollection<BulkFulfillmentOrderViewModel> Orders { get; set; }

        public ICollection<BulkFulfillmentLoadViewModel> Loads { get; set; }

        public ICollection<ShipmentBookingReferenceViewModel> Shipments { get; set; }

        public ICollection<POFulfillmentItineraryViewModel> Itineraries { get; set; }
        public string ViewSettingModuleId { get; set; } = Core.Models.ViewSettingModuleId.BULKBOOKING_DETAIL;
        public IEnumerable<ViewSettingDataSourceViewModel> ViewSettings { get ; set ; }
        public string PoRemark { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {

        }
    }
}
