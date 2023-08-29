using Groove.CSFE.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.CSFE.Persistence.Configurations
{
    public class OrganizationInRoleModelConfiguration : IEntityTypeConfiguration<OrganizationInRoleModel>
    {
        public void Configure(EntityTypeBuilder<OrganizationInRoleModel> builder)
        {
            builder.HasKey(tb => new { tb.OrganizationId, tb.OrganizationRoleId });
            builder.HasOne(rt => rt.Organization)
               .WithMany(cc => cc.OrganizationInRoles)
               .HasForeignKey(rt => rt.OrganizationId)
               .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(rt => rt.OrganizationRole)
              .WithMany(cc => cc.OrganizationInRoles)
              .HasForeignKey(rt => rt.OrganizationRoleId)
              .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
