namespace Groove.SP.Core.Entities
{
    public class ShipmentLoadDetailModel : Entity
    {
        public long Id { get; set; }

        public long? ShipmentId { get; set; }

        public long? ConsignmentId { get; set; }

        public long? CargoDetailId { get; set; }

        public long? ShipmentLoadId { get; set; }

        public long? ContainerId { get; set; }

        public long? ConsolidationId { get; set; }

        public decimal Package { get; set; }

        public string PackageUOM { get; set; }

        public decimal Unit { get; set; }

        public string UnitUOM { get; set; }

        public decimal Volume { get; set; }

        public string VolumeUOM { get; set; }

        public decimal GrossWeight { get; set; }

        public string GrossWeightUOM { get; set; }

        public decimal? NetWeight { get; set; }

        public string NetWeightUOM { get; set; }

        public int? Sequence { get; set; }

        public virtual CargoDetailModel CargoDetail { get; set; }

        public virtual ShipmentLoadModel ShipmentLoad { get; set; }
        
        public virtual ContainerModel Container { get; set; }

        public virtual ConsolidationModel Consolidation { get; set; }

        public virtual ShipmentModel Shipment { get; set; }

        public virtual ConsignmentModel Consignment { get; set; }
    }
}
