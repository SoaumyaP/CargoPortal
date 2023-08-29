using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Groove.CSFE.Core.Entities
{
    public class OrganizationRoleModel : Entity
    {
        public long Id { get; set; }
        
        public string Name { get; set; }

        public string Description { get; set; }

        public OrganizationType OrganizationTypes { get; set; }

        public virtual ICollection<OrganizationInRoleModel> OrganizationInRoles { get; set; }
    }
}
