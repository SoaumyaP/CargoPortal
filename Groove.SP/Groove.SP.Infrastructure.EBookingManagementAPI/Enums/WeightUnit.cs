using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Groove.SP.Infrastructure.EBookingManagementAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum WeightUnit
    {
        /// <summary>
        /// Kilogram
        /// </summary>
        KG,

        /// <summary>
        /// Ounce
        /// </summary>
        OZ,

        /// <summary>
        /// Pound
        /// </summary>
        LB,
    }
}
