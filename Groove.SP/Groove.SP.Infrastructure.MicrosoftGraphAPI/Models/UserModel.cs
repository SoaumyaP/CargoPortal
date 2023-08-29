using System.Collections.Generic;
using Microsoft.Graph;
using Newtonsoft.Json;


namespace Groove.SP.Infrastructure.MicrosoftGraphAPI
{
    public class UserModel : User
    {
        //
        // Summary:
        //     Represents the identities that can be used to sign in to this user account.
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "identities")]
        public IEnumerable<ObjectIdentity> Identities { get; set; }
    }

    public class ObjectIdentity
    {
        [JsonProperty(PropertyName = "signInType")]
        public string SignInType { get; set; }

        [JsonProperty(PropertyName = "issuer")]
        public string Issuer { get; set; }

        [JsonProperty(PropertyName = "issuerAssignedId")]
        public string IssuerAssignedId { get; set; }
    }
}
