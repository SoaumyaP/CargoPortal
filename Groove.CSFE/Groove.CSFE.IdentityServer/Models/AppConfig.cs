using System.Collections.Generic;

namespace Groove.CSFE.IdentityServer.Models
{
    public class AppConfig
    {
        public ICollection<Client> Clients { get; set; }

        public ICollection<API> APIs { get; set; }

        public class Client
        {
            public Client()
            {
                RedirectUris = new List<string>();
                PostLogoutRedirectUris = new List<string>();
                AllowedCorsOrigins = new List<string>();
                AllowedScopes = new List<string>();
            }

            public string ClientId { get; set; }
            public string ClientName { get; set; }
            public string GrantType { get; set; }
            public bool IsImportClient { get; set; }
            public ICollection<string> RedirectUris { get; set; }
            public ICollection<string> PostLogoutRedirectUris { get; set; }
            public ICollection<string> AllowedCorsOrigins { get; set; }
            public ICollection<string> AllowedScopes { get; set; }
            public ICollection<string> ClientSecrets { get; set; }
            public int AccessTokenLifetimeInSecond { get; set; }
            public int IdentityTokenLifetimeInSecond { get; set; }
            
        }

        public class API
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public ICollection<string> Secrets { get; set; }
            public ICollection<string> Scopes { get; set; }
        }
    }
}
