using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Groove.SP.Infrastructure.EBookingManagementAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PayableAt
    {
        /// <summary>
        /// Destination
        /// </summary>
        Destination,

        /// <summary>
        /// Origin
        /// </summary>
        Origin
    }
}
