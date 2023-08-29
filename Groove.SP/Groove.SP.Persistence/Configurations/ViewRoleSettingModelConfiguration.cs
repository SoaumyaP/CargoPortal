using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Groove.SP.Persistence.Configurations
{
    public class ViewRoleSettingModelConfiguration : IEntityTypeConfiguration<ViewRoleSettingModel>
    {
        public void Configure(EntityTypeBuilder<ViewRoleSettingModel> builder)
        {
            builder.HasKey(x => new { x.RoleId, x.ViewId });

            builder.HasOne(x => x.Role)
                .WithMany(x => x.ViewRoleSettings)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.ViewSetting)
                .WithMany(x => x.ViewRoleSettings)
                .HasForeignKey(x => x.ViewId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
