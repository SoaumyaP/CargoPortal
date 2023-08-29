using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Groove.SP.Infrastructure.EBookingManagementAPI
{

    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MovementType
    {
        [EnumMember(Value = "CY/CY")]
        CY_CY = 1 << 0,
        [EnumMember(Value = "CFS/CY")]
        CFS_CY = 1 << 1,
        [EnumMember(Value = "CY/CFS")]
        CY_CFS = 1 << 2,
        [EnumMember(Value = "CFS/CFS")]
        CFS_CFS = 1 << 3
    }
}
