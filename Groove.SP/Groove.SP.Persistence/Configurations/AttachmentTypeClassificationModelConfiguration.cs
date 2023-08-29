using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class AttachmentTypeClassificationModelConfiguration : IEntityTypeConfiguration<AttachmentTypeClassificationModel>
    {
        public void Configure(EntityTypeBuilder<AttachmentTypeClassificationModel> builder)
        {
            builder.Property(e => e.AttachmentType).HasColumnType("NVARCHAR(64)").IsRequired();
            builder.Property(e => e.EntityType).HasColumnType("NVARCHAR(3)").IsRequired();

            builder.ToTable("AttachmentTypeClassifications", "dbo");

            builder.HasIndex(e => new { e.EntityType, e.AttachmentType, e.Order }).IsUnique();
        }
    }
}
