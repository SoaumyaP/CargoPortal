using System.Collections.Generic;

namespace Groove.SP.Core.Entities
{
    public class ShipmentLoadModel : Entity
    {
        public long Id { get; set; }

        public long? ShipmentId { get; set; }

        public long? ConsignmentId { get; set; }

        public long? ConsolidationId { get; set; }

        public long? ContainerId { get; set; }

        public string ModeOfTransport { get; set; }

        public string CarrierBookingNo { get; set; }

        public bool IsFCL { get; set; }

        public string LoadingPlace { get; set; }

        // OrganizationId
        public long? LoadingPartyId { get; set; }

        public string EquipmentType { get; set; }

        public virtual ShipmentModel Shipment { get; set; }

        public virtual ConsignmentModel Consignment { get; set; }

        public virtual ConsolidationModel Consolidation { get; set; }

        public virtual ContainerModel Container { get; set; }

        public virtual ICollection<ShipmentLoadDetailModel> ShipmentLoadDetails { get; set; }

        public virtual ICollection<BillOfLadingShipmentLoadModel> BillOfLadingShipmentLoads { get; set; }
    }
}
