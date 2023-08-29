using System;

namespace Groove.SP.Core.Entities
{
    public class ShipmentBillOfLadingModel : Entity
    {
        public long ShipmentId { get; set; }

        public long BillOfLadingId { get; set; }



        public virtual ShipmentModel Shipment { get; set; }

        public virtual BillOfLadingModel BillOfLading { get; set; }
    }
}
