using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Groove.SP.Infrastructure.EBookingManagementAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Status
    {
        /// <summary>
        /// Import
        /// </summary>
        N,

        /// <summary>
        /// Update (after duplicated error)
        /// </summary>
        C,

        /// <summary>
        /// Cancel
        /// </summary>
        D
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum EeSIStatus
    {
        /// <summary>
        /// Plan to Ship
        /// </summary>
        N,

        /// <summary>
        /// Submit Re-load
        /// </summary>
        C,
    }
}
