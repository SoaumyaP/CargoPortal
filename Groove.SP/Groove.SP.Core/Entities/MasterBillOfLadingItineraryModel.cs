namespace Groove.SP.Core.Entities
{
    public class MasterBillOfLadingItineraryModel : Entity
    {
        public long ItineraryId { get; set; }

        public long MasterBillOfLadingId { get; set; }



        public ItineraryModel Itinerary { get; set; }

        public MasterBillOfLadingModel MasterBillOfLading { get; set; }
    }
}
