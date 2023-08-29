using System.Collections.Generic;

namespace Groove.SP.Core.Entities
{
    public class PermissionModel : Entity
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public virtual ICollection<RolePermissionModel> RolePermissions { get; set; }
    }
}