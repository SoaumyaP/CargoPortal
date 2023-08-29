using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Groove.SP.Infrastructure.EBookingManagementAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PartyType
    {
        /// <summary>
        /// Shipper
        /// </summary>
        SHIPPER,

        /// <summary>
        /// Consignee
        /// </summary>
        CONSIGNEE,

        /// <summary>
        /// Notify 1
        /// </summary>
        NOTIFY1,

        /// <summary>
        /// Notify 2
        /// </summary>
        NOTIFY2,

        /// <summary>
        /// Origin Agent
        /// </summary>
        ORIGINAGENT
    }
}
