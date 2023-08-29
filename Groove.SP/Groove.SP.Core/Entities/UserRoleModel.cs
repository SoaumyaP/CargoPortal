namespace Groove.SP.Core.Entities
{
    public class UserRoleModel : Entity
    {
        public long UserId { get; set; }

        public long RoleId { get; set; }

        public UserProfileModel User { get; set; }

        public RoleModel Role { get; set; }
    }
}
