namespace Groove.SP.Core.Entities
{
    public class ConsignmentItineraryModel : Entity
    {
        public long ConsignmentId { get; set; }

        public long ItineraryId { get; set; }

        public long? ShipmentId { get; set; }

        public long? MasterBillId { get; set; }

        public int Sequence { get; set; }


        /// <summary>
        /// To define linkage between shipment and master bill of lading
        /// </summary>
        public ConsignmentModel Consignment { get; set; }

        public ItineraryModel Itinerary { get; set; }

        public ShipmentModel Shipment { get; set; }

        public MasterBillOfLadingModel MasterBill { get; set; }
    }
}
