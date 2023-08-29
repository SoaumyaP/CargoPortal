using Groove.SP.Application.Shipments.ViewModels;
using Groove.SP.Application.ViewSetting.Interfaces;
using Groove.SP.Application.ViewSetting.ViewModels;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Groove.SP.Application.POFulfillment.ViewModels
{
    public class SummaryPOFulfillmentViewModel : IHasViewSetting
    {
        public long Id { get; set; }

        public string Number { get; set; }

        public int FulfillmentUnitQty { get; set; }

        public string ShipFromName { get; set; }

        public string ShipToName { get; set; }

        public DateTime? ExpectedShipDate { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }

        public POFulfillmentStage Stage { get; set; }

        public OrderFulfillmentPolicy OrderFulfillmentPolicy { get; set; }

        public string SONo { get; set; }
        public POFulfillmentStatus Status {get;set;}
        public List<ShipmentReferenceViewModel> Shipments { get; set; }

        [JsonIgnore]
        public string ViewSettingModuleId { get; set; } = Core.Models.ViewSettingModuleId.PO_DETAIL_BOOKINGS;

        public IEnumerable<ViewSettingDataSourceViewModel> ViewSettings { get; set; }
    }
}