namespace Groove.SP.Core.Entities
{
    public class ContainerItineraryModel : Entity
    {
        public long ItineraryId { get; set; }

        public long ContainerId { get; set; }



        public virtual ItineraryModel Itinerary { get; set; }

        public virtual ContainerModel Container { get; set; }
    }
}
