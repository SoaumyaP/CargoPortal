namespace Groove.SP.Application.CargoDetail.ViewModels
{
    public class CargoDetailListViewModel
    {
        public long Id { get; set; }

        public string ShippingMarks { get; set; }

        public string Description { get; set; }

        public decimal? Unit { get; set; }

        public string UnitUOM { get; set; }

        public decimal? Package { get; set; }

        public string PackageUOM { get; set; }

        public decimal? Volume { get; set; }

        public string VolumeUOM { get; set; }

        public decimal? GrossWeight { get; set; }

        public string GrossWeightUOM { get; set; }

        public string Commodity { get; set; }

        public string HSCode { get; set; }

        public long? PurchaseOrderId { get; set; }

        public string CustomerPONumber { get; set; }

        public long? POLineItemId { get; set; }

        public string ProductCode { get; set; }

        public int? LineOrder { get; set; }

        public int OrderType { get; set; }
    }
}
