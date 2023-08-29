namespace Groove.CSFE.IdentityServer.Extensions
{
    public class AzureAdConfig
    {
        public string ClientId { get; set; }

        public string Instance { get; set; }

        public string Domain { get; set; }

        public string TenantId { get; set; }

        public string CallbackPath { get; set; }

        public string SignedOutCallbackPath { get; set; }

        public string Authority => $"{Instance}{TenantId}/v2.0";

        public string SignedOutRedirectUri { get; set; }

        public bool CheckInternalUser(string tenantId)
        {
            if (string.IsNullOrEmpty(tenantId))
            {
                return false;
            }
            return TenantId == tenantId;
        }

    }
}
