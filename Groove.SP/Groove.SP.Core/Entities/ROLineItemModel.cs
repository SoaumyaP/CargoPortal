using Groove.SP.Core.Models;

namespace Groove.SP.Core.Entities
{
    public class ROLineItemModel : Entity
    {
        public long Id { get; set; }
        public long RoutingOrderId { get; set; }
        public string PONo { get; set; }
        public string ItemNo { get; set; }
        public string DescriptionOfGoods { get; set; }
        public string ChineseDescription { get; set; }
        public int OrderedUnitQty { get; set; }
        public UnitUOMType UnitUOM { get; set; }
        public int? BookedPackage { get; set; }
        public PackageUOMType? PackageUOM { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? NetWeight { get; set; }
        public decimal? Volume { get; set; }
        public string HsCode { get; set; }
        public string Commodity { get; set; }
        public string ShippingMarks { get; set; }
        public string CountryCodeOfOrigin { get; set; }

        public virtual RoutingOrderModel RoutingOrder { get; set; }
    }
}
