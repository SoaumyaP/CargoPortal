
using Groove.SP.Application.Converters;
using Newtonsoft.Json;
using System;

namespace Groove.SP.Application.POFulfillmentCargoReceive.ViewModels
{
    public class WarehouseCargoReceiveMobileModel
    {
        public string Location { get; set; }
        public int Stage { get; set; }
        [JsonConverter(typeof(UtcDateTimeConverter))]
        public DateTime? CargoReceivedDate { get; set; }

    }

    public class WarehouseCargoReceiveDateMobileModel
    {
        [JsonConverter(typeof(UtcDateTimeConverter))]
        public DateTime CargoReceivedDate { get; set; }
    }
}
