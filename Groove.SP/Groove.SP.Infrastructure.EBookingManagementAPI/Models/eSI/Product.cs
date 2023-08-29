using Newtonsoft.Json;
using System.Collections.Generic;

namespace Groove.SP.Infrastructure.EBookingManagementAPI.eSI
{
    public class Product
    {
        public string Line { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
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
        public string Length { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Width { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Height { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Currency { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string UnitPrice { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ShipmentNo { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DestCode { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ItemRef2 { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string HSCode { get; set; }

        public IList<Packing> PackingList { get; set; }
    }
}
