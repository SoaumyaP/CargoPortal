using Groove.SP.Application.ViewSetting.Interfaces;
using Groove.SP.Application.ViewSetting.ViewModels;
using System;
using System.Collections.Generic;

namespace Groove.SP.Application.Shipments.ViewModels
{
    public class ShipmentBookingReferenceViewModel : IHasViewSetting
    {
        public long Id { get; set; }

        public string ShipmentNo { get; set; }

        public string ShipFrom { get; set; }

        public DateTime ShipFromETDDate { get; set; }

        public string ShipTo { get; set; }

        public DateTime ShipToETADate { get; set; }

        public string Status { get; set; }

        public string ViewSettingModuleId { get; set; } = Core.Models.ViewSettingModuleId.FREIGHTBOOKING_DETAIL_SHIPMENT;

        public IEnumerable<ViewSettingDataSourceViewModel> ViewSettings { get; set; }
    }
}