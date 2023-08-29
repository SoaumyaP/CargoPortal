using System.Collections.Generic;

namespace Groove.SP.Infrastructure.CSFE.Models
{
    public class CountryLocations
    {
        public long CountryId { get; set; }
        public List<long> LocationIds { get; set; }
    }
}
