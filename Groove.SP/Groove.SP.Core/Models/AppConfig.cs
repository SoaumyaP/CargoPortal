namespace Groove.SP.Core.Models
{
    public class AppConfig
    {
        public string ApiUrl { get; set; }

        public string InternalDomain { get; set; }

        public string InternalOrganization { get; set; }

        public string AdminAccount { get; set; }

        public string EmailSenderName { get; set; }

        public string EmailSenderAddress { get; set; }

        public string EmailSenderUsername { get; set; }

        public string EmailSenderPassword { get; set; }
        public string CSREmailDomain { get; set; }

        public int SmtpPort { get; set; }

        public string SmtpHost { get; set; }

        public string MarketingUrl { get; set; }

        public string InternalIdentityType { get; set; }

        public string ExternalIdentityType { get; set; }

        public string InternalIdentityTenant { get; set; }

        public string ExternalIdentityTenant { get; set; }

        public decimal TEUCBMRatio { get; set; }

        public CORSConfig CORS { get; set; }

        public DataAccessConfig DataAccess { get; set; }

        public EmailConfig Email { get; set; }

        public string SupportEmail { get; set; }
        public string ImportConsigneeOrganizationEmail { get; set; }

        public AzureConfig Azure { get; set; } = new AzureConfig();

        public BlobStorageConfig BlobStorage { get; set; }

        public string ClientUrl { get; set; }

        public ReportConfig Report { get; set; } = new ReportConfig();

        public B2CConfig B2C { get; set; }

        public HangfireConfig Hangfire { get; set; }
        public TelemetryConfig Telemetry { get; set; }

        /// <summary>
        /// Zero to disable cache
        /// </summary>
        public int AppDataMemoryCacheInSeconds { get; set; }
    }

    public class CORSConfig
    {
        public string Origins { get; set; }
    }

    public class ReportConfig
    {
        public string ReportServerUrl { get; set; }

        public string ReportUsername { get; set; }

        public string ReportPassword { get; set; }

        public string SystemUserPassword { get; set; }

        public string WebhookSecret { get; set; }

        public string SystemUserRoleIds { get; set; }
    }

    public class DataAccessConfig
    {
        public DataAccessInvoiceConfig Invoice { get; set; }
    }

    public class DataAccessConsignmentConfig
    {
        public int Before { get; set; }

        public int After { get; set; }
    }

    public class DataAccessInvoiceConfig
    {
        public int Before { get; set; }

        public int After { get; set; }
    }

    public class DataAccessShipmentConfig
    {
        public int Before { get; set; }

        public int After { get; set; }
    }

    public class DataAccessMasterBOLConfig
    {
        public int Before { get; set; }

        public int After { get; set; }
    }

    public class DataAccessFreightSchedulerConfig
    {
        public int Before { get; set; }

        public int After { get; set; }
    }

    public class DataAccessContainerConfig
    {
        public int Before { get; set; }

        public int After { get; set; }
    }

    public class BlobStorageConfig
    {
        public string FileSystemBlobStorageLocation { get; set; }

        public string AzureStorageConnectionString { get; set; }
        public string FromDate { set; get; }
        public string ToDate { set; get; }
        public int DefaultDeletedItem { set; get; }
        public string PurchaseOrderTemplate { get; set; }
        public POFulfillmentTemplate POFulfillment { get; set; }

    }

    public class POFulfillmentTemplate
    {
        public string PackingListTemplate { get; set; }
        public string ShippingInvoiceTemplate { get; set; }
        public string SOFormTemplate { get; set; }
    }

    public class AzureConfig
    {
        public int AzureBlobRetryBackoffTime { get; set; } = 1;

        public int AzureBlobMaxRetryAttempts { get; set; } = 3;

        public int AzureQueueRetryBackoffTime { get; set; } = 1;

        public int AzureQueueMaxRetryAttempts { get; set; } = 3;
    }

    public class EmailConfig
    {
        public int AttachmentExpiredTime { get; set; }
    }

    public class B2CConfig
    {
        public string Tenant { get; set; }
        public string Policy { get; set; }
        public string ResetPasswordPolicyId { get; set; }
        public int PolicyTokenLifeTime { get; set; }
        public string ClientId { get; set; }
        public string RedirectUri { get; set; }
        public string PasswordResetUrl { get; set; }
        public string ClientSecret { get; set; }
    }

    public class EBookingManagementAPIConfig
    {
        public string Id { get; set; }
        public string Password { get; set; }
        public string APIEndpoint { get; set; }
    }

    public class HangfireConfig
    {
        public string DashboardUrl { get; set; }

        public int JobRetentionTimeInDay { get; set; } = 1;
    }

    public class TelemetryConfig
    {
        public string Key { get; set; }

        public string Source { get; set; } = "N/A";
    }

    public class CSEDShippingDocumentServiceBus
    {
        public string ConnectionString { get; set; }
        public string Topic { get; set; }
        public string Subscription { get; set; }
    }

    public class CSEDShippingDocumentCredential
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Authority { get; set; }
        public string[] Scopes { get; set; }
    }

    public class SFTPRoutingOrderServerProfile
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string BlobKeyId { get; set; }
        public string ImportDirectory { get; set; }
        public string ArchiveDirectory { get; set; }
    }

    public class AppDbConnections
    {
        public string CsPortalDb { get; set; }
        public string SecondaryCsPortalDb { get; set; }
        public string HangfireDb { get; set; }
    }

    public class CustomerOrgReference
    {
        public string TUMIOrgCode { get; set; }
    }

    public enum AppDbConnectionName
    {
        CsPortalDb,
        SecondaryCsPortalDb
    }
}
