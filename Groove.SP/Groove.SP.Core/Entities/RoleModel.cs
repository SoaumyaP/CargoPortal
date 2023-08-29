using Groove.SP.Core.Models;
using System.Collections.Generic;

namespace Groove.SP.Core.Entities
{
    public class RoleModel : Entity
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Activated { get; set; }
        public bool IsInternal { get; set; }
        public RoleStatus Status { get; set; }
        public bool IsOfficial { get; set; }

        public virtual ICollection<UserRoleModel> UserRoles { get; set; }
        public virtual ICollection<RolePermissionModel> RolePermissions { get; set; }
        public virtual ICollection<ViewRoleSettingModel> ViewRoleSettings { get; set; }
    }
}