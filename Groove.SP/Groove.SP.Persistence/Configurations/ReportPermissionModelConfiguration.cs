using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class ReportPermissionModelConfiguration : IEntityTypeConfiguration<ReportPermissionModel>
    {
        public void Configure(EntityTypeBuilder<ReportPermissionModel> builder)
        {
            builder.Property(e => e.OrganizationIds).HasColumnType("nvarchar(1024)");
            builder.Property(e => e.RoleId).IsRequired();

            builder.HasOne(e => e.Report)
                    .WithMany(e => e.Permissions)
                    .HasForeignKey(e => e.ReportId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<RoleModel>()
                .WithMany()
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(e => new { e.ReportId, e.OrganizationIds, e.RoleId }).IsUnique();

        }
    }
}
