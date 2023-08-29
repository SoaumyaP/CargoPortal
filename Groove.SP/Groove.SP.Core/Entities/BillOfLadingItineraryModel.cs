namespace Groove.SP.Core.Entities
{
    public class BillOfLadingItineraryModel : Entity
    {
        public long ItineraryId { get; set; }

        public long BillOfLadingId { get; set; }

        public long? MasterBillOfLadingId { get; set; }

        public ItineraryModel Itinerary { get; set; }

        public BillOfLadingModel BillOfLading { get; set; }

        public MasterBillOfLadingModel MasterBillOfLading { get; set; }
    }
}
