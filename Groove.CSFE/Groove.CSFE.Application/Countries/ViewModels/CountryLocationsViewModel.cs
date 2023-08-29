using System.Collections.Generic;
using Groove.CSFE.Application.Converters;
using Newtonsoft.Json;

namespace Groove.CSFE.Application.Countries.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class CountryLocationsViewModel
    {
        public long CountryId { get; set; }
        public List<long> LocationIds { get; set; }
    }
}
