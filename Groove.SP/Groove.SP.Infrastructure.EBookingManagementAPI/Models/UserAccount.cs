using Newtonsoft.Json;

namespace Groove.SP.Infrastructure.EBookingManagementAPI
{
    public class UserAccount
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }
    }
}
