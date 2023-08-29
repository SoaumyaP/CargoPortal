using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class AttachmentTypePermissionModelConfiguration : IEntityTypeConfiguration<AttachmentTypePermissionModel>
    {
        public void Configure(EntityTypeBuilder<AttachmentTypePermissionModel> builder)
        {
            builder.Property(e => e.AttachmentType).HasColumnType("NVARCHAR(64)").IsRequired();
            builder.Property(e => e.RoleId).IsRequired();
            builder.Property(e => e.Alias).HasMaxLength(100);

            builder.HasOne<RoleModel>()
                .WithMany()
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("AttachmentTypePermissions", "dbo");

            builder.HasIndex(e => new { e.RoleId, e.AttachmentType }).IsUnique();
        }
    }
}
