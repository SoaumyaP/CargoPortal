namespace Groove.SP.Application.CargoDetail.ViewModels
{
    public class CargoDetailLoadViewModel
    {
        public long Id { get; set; }

        public long ShipmentId { get; set; }

        public string ShipmentNo { get; set; }

        public long OrderId { get; set; }

        public string PONumber { get; set; }

        public long ItemId { get; set; }

        public string ProductCode { get; set; }

        public decimal? Unit { get; set; }

        public string UnitUOM { get; set; }

        public decimal? Package { get; set; }

        public string PackageUOM { get; set; }

        public decimal? Volume { get; set; }

        public string VolumeUOM { get; set; }

        public decimal GrossWeight { get; set; }

        public string GrossWeightUOM { get; set; }

        public decimal? NetWeight { get; set; }

        public string NetWeightUOM { get; set; }

        // Load Info
        public int Sequence { get; set; }

        public long? ConsignmentId { get; set; }

        public long? ContainerId { get; set; }

        public long ShipmentLoadId { get; set; }
    }
}