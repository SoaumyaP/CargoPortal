namespace Groove.CSFE.IdentityServer.Models
{
    public class BusinessUserInformation
    {
        /// <summary>
        /// The name of user stored in the Portal database
        /// </summary>
        public string PortalUserName { get; set; }
        public long OrganizationId { get; set; }
        public long UserRoleId { get; set; }
        public string UserRoleName { get; set; }
        /// <summary>
        /// Whether current user is internal
        /// </summary>
        public bool IsInternal { get; set; }
    }
}
