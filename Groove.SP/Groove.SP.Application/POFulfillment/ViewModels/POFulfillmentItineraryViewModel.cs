using Groove.SP.Application.Common;
using Groove.SP.Application.ViewSetting.Interfaces;
using Groove.SP.Application.ViewSetting.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace Groove.SP.Application.POFulfillment.ViewModels
{
    public class POFulfillmentItineraryViewModel : ViewModelBase<POFulfillmentItineraryModel>, IHasViewSetting
    {
        public long Id { get; set; }

        public int Sequence { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ModeOfTransportType ModeOfTransport { get; set; }

        public long CarrierId { get; set; }

        public string CarrierName { get; set; }

        public string VesselFlight { get; set; }

        public long LoadingPortId { get; set; }

        public string LoadingPort { get; set; }

        public long DischargePortId { get; set; }

        public string DischargePort { get; set; }

        public DateTime? ETDDate { get; set; }

        public DateTime? ETADate { get; set; }

        public string ViewSettingModuleId { get; set; } = Core.Models.ViewSettingModuleId.FREIGHTBOOKING_DETAIL_SHIPMENT_PLANNED_SCHEDULE;

        public IEnumerable<ViewSettingDataSourceViewModel> ViewSettings { get; set; }

        public POFulfillmentItinerayStatus Status { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}
