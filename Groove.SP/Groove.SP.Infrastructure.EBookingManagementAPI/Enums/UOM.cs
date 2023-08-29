using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Groove.SP.Infrastructure.EBookingManagementAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UOM
    {
        /// <summary>
        /// Each
        /// </summary>
        EA = 10,

        /// <summary>
        /// Pair
        /// </summary>
        PA = 20,           

        /// <summary>
        /// Set
        /// </summary>
        ST = 30,

        /// <summary>
        /// Piece
        /// </summary>
        PC = 40,
    }
}
