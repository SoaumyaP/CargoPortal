using Groove.SP.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Groove.SP.Application.Shipments.ViewModels
{
    public class ShipmentItemViewModel
    {
        public long Id { get; set; }

        public long? POLineItemId { get; set; }

        public long? PurchaseOrderId { get; set; }

        public string CustomerPONumber { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public int OrderedUnitQty { get; set; }

        public int FulfillmentUnitQty { get; set; }

        public int BalanceUnitQty { get; set; }

        public int LoadedQty { get; set; }

        public int OpenQty { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public UnitUOMType UnitUOM { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PackageUOMType PackageUOM { get; set; }

        public string Commodity { get; set; }

        public string HSCode { get; set; }

        public string CountryCodeOfOrigin { get; set; }

        public long ShipmentId { get; set; }

        public int? BookedPackage { get; set; }

        public decimal? Volume { get; set; }

        public decimal? GrossWeight { get; set; }

        public decimal? NetWeight { get; set; }

        public int? InnerQuantity { get; set; }

        public int? OuterQuantity { get; set; }
    }
}
