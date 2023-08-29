using Groove.SP.Application.Common;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Groove.SP.Application.POFulfillment.ViewModels
{
    public class POFulfillmentCargoDetailViewModel
    {
        public long Id { get; set; }

        public long POFulfillmentId { get; set; }

        public int LineOrder { get; set; }

        public decimal Height { get; set; }

        public decimal Width { get; set; }

        public decimal Length { get; set; }

        public DimensionUnitType DimensionUnit { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public UnitUOMType UnitUOM { get; set; }

        public int PackageQuantity { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PackageUOMType PackageUOM { get; set; }

        public decimal Volume { get; set; }

        public decimal GrossWeight { get; set; }

        public decimal NetWeight { get; set; }

        public string CountryCodeOfOrigin { get; set; }

        public string HsCode { get; set; }

        public string ShippingMarks { get; set; }

        public string PackageDescription { get; set; }

        public int UnitQuantity { get; set; }

        public string LoadReferenceNumber { get; set; }

        public string Commodity { get; set; }
    }
}