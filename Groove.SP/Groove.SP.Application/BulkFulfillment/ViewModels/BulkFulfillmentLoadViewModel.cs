using System;
using System.Collections.Generic;
using System.Text;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Application.ViewSetting.Interfaces;
using Groove.SP.Application.ViewSetting.ViewModels;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Groove.SP.Application.BulkFulfillment.ViewModels
{
    public class BulkFulfillmentLoadViewModel: IHasViewSetting
    {
        public long Id { get; set; }

        public string LoadReferenceNumber { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public EquipmentType EquipmentType { get; set; }

        public decimal PlannedVolume { get; set; }

        public decimal? PlannedNetWeight { get; set; }

        public decimal PlannedGrossWeight { get; set; }

        public int PlannedPackageQuantity { get; set; }

        public int SubtotalPackageQuantity { get; set; }

        public int SubtotalUnitQuantity { get; set; }

        public decimal SubtotalNetWeight { get; set; }

        public decimal SubtotalGrossWeight { get; set; }

        public decimal SubtotalVolume { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PackageUOMType PackageUOM { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public POFulfillmentLoadStatus Status { get; set; }

        public ICollection<POFulfillmentLoadDetailViewModel> Details { get; set; }


        // update container
        public string ContainerNumber { get; set; }

        public string SealNumber { get; set; }

        public string SealNumber2 { get; set; }

        public DateTime? LoadingDate { get; set; }

        public DateTime? GateInDate { get; set; }

        public decimal? TotalGrossWeight { get; set; }

        public decimal? TotalNetWeight { get; set; }

        public string ViewSettingModuleId { get; set; } = Core.Models.ViewSettingModuleId.BULKBOOKING_DETAIL_LOADS;

        public IEnumerable<ViewSettingDataSourceViewModel> ViewSettings { get; set; }
    }
}
