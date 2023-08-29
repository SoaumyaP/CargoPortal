using Groove.SP.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Groove.SP.Application.PurchaseOrders.ViewModels
{
    public class BookingPOLineItemViewModel
    {
        public long Id { get; set; }
        public int LineOrder { get; set; }
        public int? OrderedUnitQty { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string DescriptionOfGoods { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public UnitUOMType UnitUOM { get; set; }
        public int? BookedUnitQty { get; set; }
        public int? BalanceUnitQty { get; set; }
        public decimal? UnitPrice { get; set; }
        public string CurrencyCode { get; set; }
        public string HSCode { get; set; }
        public string ChineseDescription { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public PackageUOMType? PackageUOM { get; set; }
        public string CountryCodeOfOrigin { get; set; }
        public string Commodity { get; set; }
        public string ShippingMarks { get; set; }
        public string GridValue { get; set; }

        public long PurchaseOrderId { get; set; }

        #region Article Master info
        public int? OuterQuantity { get; set; }
        public int? InnerQuantity { get; set; }
        public decimal? OuterDepth { get; set; }
        public decimal? OuterHeight { get; set; }
        public decimal? OuterWidth { get; set; }
        public decimal? OuterGrossWeight { get; set; }
        #endregion
    }
}
