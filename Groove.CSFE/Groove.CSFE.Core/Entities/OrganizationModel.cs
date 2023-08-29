using System.Collections.Generic;

namespace Groove.CSFE.Core.Entities
{
    public class OrganizationModel : Entity
    {
        public long Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }
        
        public string ContactEmail { get; set; }
        
        public string ContactName { get; set; }
        
        public string ContactNumber { get; set; }

        public string Address { get; set; }

        public string AddressLine2 { get; set; }

        public string AddressLine3 { get; set; }

        public string AddressLine4 { get; set; }

        public string WeChatOrWhatsApp { get; set; }

        public string EdisonInstanceId { get; set; }

        public string EdisonCompanyCodeId { get; set; }

        public string CustomerPrefix { get; set; }

        public long? LocationId { get; set; }

        public string OrganizationLogo { get; set; }

        public string TaxpayerId { get; set; }

        public virtual LocationModel Location { get; set; }

        public OrganizationType OrganizationType { get; set; }

        public AgentType AgentType { get; set; }
        
        public string ParentId { get; set; }

        public OrganizationStatus Status { get; set; }

        public bool IsBuyer { get; set; }

        public SOFormGenerationFileType SOFormGenerationFileType { get; set; }

        public virtual ICollection<OrganizationInRoleModel> OrganizationInRoles { get; set; }

        public virtual ICollection<CustomerRelationshipModel> CustomerRelationship { get; set; }

        public virtual ICollection<WarehouseLocationModel> Warehouses { get; set; }

        public virtual ICollection<WarehouseAssignmentModel> WarehouseAssignments { get; set; }

        public virtual ICollection<EmailNotificationModel> EmailNotifications { get; set; }

        public string GetQueryParentIdString() { return this.ParentId + this.Id + AppConstants.DELIMITER_PARENT_ID; }

        public string AdminUser { get; set; }

        public string WebsiteDomain { get; set; }

    }
}
