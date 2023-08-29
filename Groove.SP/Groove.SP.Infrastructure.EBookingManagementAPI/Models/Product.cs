
using Newtonsoft.Json;

namespace Groove.SP.Infrastructure.EBookingManagementAPI
{
    public class Product
    {
        public string Line { get; set; }

        public string OrderNumber { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string OrderSequence { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ProductNumber { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string MarksNos { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string GoodsDesc { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public QuantityUnit? QuantityUnit { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Quantity { get; set; }

        public UOM UOM { get; set; }

        public string Piece { get; set; }

        public WeightUnit GrossWeightUnit { get; set; }

        public string GrossWeight { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public WeightUnit? NetWeightUnit { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string NetWeight { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CBM { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Currency { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string UnitPrice { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string HSCode { get; set; }
    }
}
