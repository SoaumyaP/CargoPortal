namespace Groove.SP.Core.Entities
{
    public class BillOfLadingShipmentLoadModel : Entity
    {
        public long BillOfLadingId { get; set; }

        public long ShipmentLoadId { get; set; }

        public long? ContainerId { get; set; }

        public long? ConsolidationId { get; set; }

        public long? MasterBillOfLadingId { get; set; }

        public bool IsFCL { get; set; }

        public virtual BillOfLadingModel BillOfLading { get; set; }

        public virtual ShipmentLoadModel ShipmentLoad { get; set; }

        public virtual ContainerModel Container { get; set; }

        public virtual ConsolidationModel Consolidation { get; set; }

        public virtual MasterBillOfLadingModel MasterBillOfLading { get; set; }
    }
}
