

namespace Groove.SP.Core.Entities
{
    public class CargoLoadDetailQueryModel
    {
        public long Id { get; set; }
        public int? Sequence { get; set; }
        public long ShipmentId { get; set; }
        public string ShipmentNo { get; set; }
        public long OrderId { get; set; }
        public string PONumber { get; set; }
        public long ItemId { get; set; }
        public string ProductCode { get; set; }
        public string CargoDescription { get; set; }
        public decimal Package { get; set; }
        public string PackageUOM { get; set; }
        public decimal Unit { get; set; }
        public string UnitUOM { get; set; }
        public decimal Volume { get; set; }
        public string VolumeUOM { get; set; }
        public decimal GrossWeight { get; set; }
        public string GrossWeightUOM { get; set; }

        //From dbo.CargoDetails
        public decimal? NetWeight { get; set; }
        public string NetWeightUOM { get; set; }

        //Balance Qty of Cargo Detail Item
        public decimal BalanceUnitQty { get; set; }
        public decimal BalancePackageQty { get; set; }
        public decimal BalanceVolume { get; set; }
        public decimal BalanceGrossWeight { get; set; }
    }
}
