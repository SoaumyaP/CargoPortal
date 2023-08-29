namespace Groove.CSFE.Core.Entities
{
    public class PortModel : Entity
    {
        public long Id { get; set; }
        public string AirportCode { get; set; }
        public string AlternativeName { get; set; }
        public string ChineseName { get; set; }
        public long CountryId { get; set; }
        public string CountryName { get; set; }
        public bool IsAirport { get; set; }
        public string Name { get; set; }
        public string SeaportCode { get; set; }
    }
}
