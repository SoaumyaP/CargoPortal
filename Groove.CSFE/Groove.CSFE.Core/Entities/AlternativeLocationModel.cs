namespace Groove.CSFE.Core.Entities
{
    public class AlternativeLocationModel : Entity
    {
        public long Id { get; set; }

        public string Name { get; set; }
        
        public long LocationId { get; set; }

        public LocationModel Location { get; set; }
    }
}