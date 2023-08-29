
namespace Groove.SP.Infrastructure.CSFE.Models
{
    public class Location
    {
        public long Id { get; set; }

        /// <summary>
        /// Location Code
        /// </summary>
        public string Name { get; set; }

        public string LocationDescription { get; set; }

        public long CountryId { get; set; }

        public Country Country { get; set; }
    }
}
