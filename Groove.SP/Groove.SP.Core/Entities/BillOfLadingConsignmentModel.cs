namespace Groove.SP.Core.Entities
{
    public class BillOfLadingConsignmentModel : Entity
    {
        public long ConsignmentId { get; set; }

        public long BillOfLadingId { get; set; }

        public long? ShipmentId { get; set; }

        public virtual ConsignmentModel Consignment { get; set; }

        public virtual BillOfLadingModel BillOfLading { get; set; }

        public virtual ShipmentModel Shipment { get; set; }
    }
}
