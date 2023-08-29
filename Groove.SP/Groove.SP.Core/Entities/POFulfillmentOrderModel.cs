using Groove.SP.Core.Models;
using System;

namespace Groove.SP.Core.Entities
{
    public class POFulfillmentOrderModel : Entity
    {
        public long Id { get; set; }
        
        public long PurchaseOrderId { get; set; }

        public long POLineItemId { get; set; }

        public long POFulfillmentId { get; set; }

        public POFulfillmentModel POFulfillment { get; set; }

        public string CustomerPONumber { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public int OrderedUnitQty { get; set; }

        public int FulfillmentUnitQty { get; set; }

        public int BalanceUnitQty { get; set; }

        public int LoadedQty { get; set; }

        public int OpenQty { get; set; }

        public UnitUOMType UnitUOM { get; set; }

        public PackageUOMType PackageUOM { get; set; }

        public string Commodity { get; set; }

        public POFulfillmentOrderStatus Status { get; set; }

        public string HsCode { get; set; }

        public string CountryCodeOfOrigin { get; set; }

        public int? BookedPackage { get; set; }

        public decimal? Volume { get; set; }

        public decimal? GrossWeight { get; set; }

        public decimal? NetWeight { get; set; }

        public string ChineseDescription { get; set; }

        public string ShippingMarks { get; set; }

        public string SeasonCode { get; set; }

        public string StyleNo { get; set; }

        public string ColourCode { get; set; }

        public string Size { get; set; }

        public decimal? Length { get; set; }

        public decimal? Width { get; set; }

        public decimal? Height { get; set; }

        public POFulfillmentCargoReceiveItemModel POFulfillmentCargoReceiveItem { get; set; }
    }
}