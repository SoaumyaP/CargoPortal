using Newtonsoft.Json;

namespace Groove.SP.Infrastructure.EBookingManagementAPI
{
    public class Party
    {
        public PartyType Type { get; set; }

        /// <summary>
        /// ediSon Code
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CompanyCode { get; set; }

        public string CompanyName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CompanyPerson { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CompanyPhone { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CompanyEmail { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CompanyAddress1 { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CompanyAddress2 { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CompanyAddress3 { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CompanyAddress4 { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CountryCode { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CityCode { get; set; }
    }
}
