using Groove.SP.Application.POFulfillmentCargoReceiveItem.ViewModels;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Groove.SP.Application.WarehouseFulfillment.ViewModels
{
    public class WarehouseFulfillmentOrderViewModel
    {
        public long Id { get; set; }

        public long PurchaseOrderId { get; set; }

        public long POLineItemId { get; set; }

        public string CustomerPONumber { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public int OrderedUnitQty { get; set; }

        public int FulfillmentUnitQty { get; set; }

        public int BalanceUnitQty { get; set; }

        public int LoadedQty { get; set; }

        public int OpenQty { get; set; }

        public bool Received { get; set; }

        public int? ReceivedQty { get; set; }

        public DateTime? ReceivedDate { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public UnitUOMType UnitUOM { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PackageUOMType PackageUOM { get; set; }

        public string Commodity { get; set; }

        public POFulfillmentOrderStatus Status { get; set; }

        public string HsCode { get; set; }

        public string CountryCodeOfOrigin { get; set; }

        public int? BookedPackage { get; set; }

        public decimal? Volume { get; set; }

        public decimal? GrossWeight { get; set; }

        public decimal? NetWeight { get; set; }

        public int? InnerQuantity { get; set; }

        public int? OuterQuantity { get; set; }

        public string ShippingMarks { get; set; }

        public string ChineseDescription { get; set; }

        public string SeasonCode { get; set; }

        public string StyleNo { get; set; }

        public string StyleName { get; set; }

        public string ColourCode { get; set; }

        public string ColourName { get; set; }

        public string Size { get; set; }

        public decimal? Length { get; set; }

        public decimal? Width { get; set; }

        public decimal? Height { get; set; }

        public POFulfillmentCargoReceiveItemViewModel POFulfillmentCargoReceiveItem { get; set; }
    }
}
