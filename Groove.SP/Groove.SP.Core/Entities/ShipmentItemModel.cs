using Groove.SP.Core.Models;

namespace Groove.SP.Core.Entities
{
    public class ShipmentItemModel : Entity
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

        public int? BookedPackage { get; set; }

        public decimal? Volume { get; set; }

        public decimal? GrossWeight { get; set; }

        public decimal? NetWeight { get; set; }

        public UnitUOMType UnitUOM { get; set; }

        public PackageUOMType PackageUOM { get; set; }

        public string Commodity { get; set; }

        public string HSCode { get; set; }

        public string CountryCodeOfOrigin { get; set; }

        public long ShipmentId { get; set; }

        public ShipmentModel Shipment { get; set; }
    }
}