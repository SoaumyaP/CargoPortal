using Groove.SP.Core.Models;

namespace Groove.SP.Core.Entities
{
    public class POLineItemModel : Entity
    {
        public long Id { get; set; }

        public string POLineKey { get; set; }

        public long PurchaseOrderId { get; set; }

        public int? LineOrder { get; set; }

        public int OrderedUnitQty { get; set; }

        public int BookedUnitQty { get; set; }

        public int BalanceUnitQty { get; set; }

        #region Product Info
        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public UnitUOMType UnitUOM { get; set; }

        public PackageUOMType? PackageUOM { get; set; }

        public string ProductFamily { get; set; }

        public string HSCode { get; set; }

        public string SupplierProductCode { get; set; }

        public int? MinPackageQty { get; set; }

        public int? MinOrderQty { get; set; }

        public decimal UnitPrice { get; set; }

        public string CurrencyCode { get; set; }

        public string Commodity { get; set; }

        public string CountryCodeOfOrigin { get; set; }

        public string SeasonCode { get; set; }

        public string StyleNo { get; set; }

        public string ColourCode { get; set; }

        public string Size { get; set; }
        #endregion

        public string ReferenceNumber1 { get; set; }

        public string ReferenceNumber2 { get; set; }

        #region Shipping Details
        public string ShippingMarks { get; set; }

        public string DescriptionOfGoods { get; set; }

        public string PackagingInstruction { get; set; }

        public string ProductRemark { get; set; }
        #endregion

        public decimal? Volume { get; set; }
        public decimal? GrossWeight { get; set; }

        #region TUMI customer
        // -> string constains 256 characters as max-length
        // -> decimal has 3 numbers in decimal place, but set data-type to decimal(18,4) to make consistent
        public int? ScheduleLineNo { get; set; }
        public string InboundDelivery { get; set; }
        public string POItemReference { get; set; }
        public string ShipmentNo { get; set; }
        public string Plant { get; set; }
        public string StorageLocation { get; set; }
        public string MatGrpDe { get; set; }
        public string MaterialType { get; set; }
        public string Sku { get; set; }
        public string GridValue { get; set; }
        public string StockCategory { get; set; }
        public string HeaderText { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? NetWeight { get; set; }
        public string FactoryName { get; set; }

        #endregion TUMI customer


        public virtual PurchaseOrderModel PurchaseOrder { get; set; }
    }
}
