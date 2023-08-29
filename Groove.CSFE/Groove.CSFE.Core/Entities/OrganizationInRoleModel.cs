namespace Groove.CSFE.Core.Entities
{
    public class OrganizationInRoleModel : Entity
    {
        public long OrganizationId { get; set; }
        
        public long OrganizationRoleId { get; set; }

        public virtual OrganizationRoleModel OrganizationRole { get; set; }

        public virtual OrganizationModel Organization { get; set; }
    }
}
