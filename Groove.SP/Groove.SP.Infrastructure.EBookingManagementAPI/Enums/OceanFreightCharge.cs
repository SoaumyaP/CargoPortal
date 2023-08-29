using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Groove.SP.Infrastructure.EBookingManagementAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OceanFreightCharge
    {
        /// <summary>
        /// Collect
        /// </summary>
        C,

        /// <summary>
        /// Prepaid
        /// </summary>
        P,
    }
}
