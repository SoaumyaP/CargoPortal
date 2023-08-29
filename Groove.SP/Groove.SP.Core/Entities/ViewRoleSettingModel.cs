namespace Groove.SP.Core.Entities
{
    public class ViewRoleSettingModel : Entity
    {
        public long RoleId { get; set; }

        public long ViewId { get; set; }

        public ViewSettingModel ViewSetting { get; set; }

        public RoleModel Role { get; set; }
    }
}