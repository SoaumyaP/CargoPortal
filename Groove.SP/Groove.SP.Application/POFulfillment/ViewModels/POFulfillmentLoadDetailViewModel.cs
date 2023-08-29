using Groove.SP.Application.ViewSetting.Interfaces;
using Groove.SP.Application.ViewSetting.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace Groove.SP.Application.POFulfillment.ViewModels
{
    public class POFulfillmentLoadDetailViewModel: IHasViewSetting
    {
        public long Id { get; set; }

        public long POFulfillmentLoadId { get; set; }

        public string CustomerPONumber { get; set; }

        public string ProductCode { get; set; }

        /// <summary>
        /// Loaded package
        /// </summary>
        public int PackageQuantity { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PackageUOMType PackageUOM { get; set; }

        public decimal? Height { get; set; }

        public decimal? Width { get; set; }

        public decimal? Length { get; set; }

        public DimensionUnitType DimensionUnit { get; set; }

        /// <summary>
        /// Loaded quantity
        /// </summary>
        public int UnitQuantity { get; set; }

        public decimal Volume { get; set; }

        public decimal GrossWeight { get; set; }

        public decimal? NetWeight { get; set; }

        public long POFulfillmentOrderId { get; set; }

        public string ShippingMarks { get; set; }

        public string PackageDescription { get; set; }

        public int? Sequence { get; set; }

        public string ViewSettingModuleId { get; set; } = Core.Models.ViewSettingModuleId.BULKBOOKING_DETAIL_LOAD_DETAILS;
        public IEnumerable<ViewSettingDataSourceViewModel> ViewSettings { get; set; }
    }
}