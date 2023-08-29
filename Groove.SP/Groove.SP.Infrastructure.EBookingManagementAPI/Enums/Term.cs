using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Groove.SP.Infrastructure.EBookingManagementAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Term
    {
        /// <summary>
        /// Port-Port
        /// </summary>
        PP = 1 << 0,

        /// <summary>
        /// Port-Door
        /// </summary>
        PD = 1 << 1,

        /// <summary>
        /// Door-Port
        /// </summary>
        DP = 1 << 2,

        /// <summary>
        /// Door-Door
        /// </summary>
        DD = 1 << 3
    }
}
