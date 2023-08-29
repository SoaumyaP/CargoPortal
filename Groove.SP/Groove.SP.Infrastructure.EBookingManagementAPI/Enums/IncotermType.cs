using System;
using System.ComponentModel.DataAnnotations;

namespace Groove.SP.Infrastructure.EBookingManagementAPI
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum IncotermType
    {
        EXW = 1 << 0,

        FCA = 1 << 1,

        CPT = 1 << 2,

        CIP = 1 << 3,

        DAT = 1 << 4,

        DAP = 1 << 5,

        DDP = 1 << 6,

        FAS = 1 << 7,

        FOB = 1 << 8,

        CFR = 1 << 9,

        CIF = 1 << 10,

        DPU = 1 << 11
    }
}
