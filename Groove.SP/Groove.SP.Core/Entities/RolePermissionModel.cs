namespace Groove.SP.Core.Entities
{
    public class RolePermissionModel : Entity
    {
        public long RoleId { get; set; }

        public long PermissionId { get; set; }

        public RoleModel Role { get; set; }

        public PermissionModel Permission { get; set; }
    }
}
