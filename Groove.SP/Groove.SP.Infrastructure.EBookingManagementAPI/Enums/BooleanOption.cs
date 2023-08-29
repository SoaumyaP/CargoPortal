using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Groove.SP.Infrastructure.EBookingManagementAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BooleanOption
    {
        /// <summary>
        /// Yes
        /// </summary>
        Y,

        /// <summary>
        /// No
        /// </summary>
        N
    }
}
