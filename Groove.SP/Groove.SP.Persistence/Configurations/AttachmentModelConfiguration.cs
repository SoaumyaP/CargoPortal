using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class AttachmentModelConfiguration : IEntityTypeConfiguration<AttachmentModel>
    {
        public void Configure(EntityTypeBuilder<AttachmentModel> builder)
        {
            builder.Property(e => e.AttachmentType).HasColumnType("NVARCHAR(64)");
            builder.Property(e => e.FileName).IsRequired().HasColumnType("NVARCHAR(256)");
            builder.Property(e => e.BlobId).IsRequired().HasColumnType("NVARCHAR(512)");
            builder.Property(e => e.ReferenceNo).HasColumnType("NVARCHAR(128)");
            builder.Property(e => e.Description).HasColumnType("NVARCHAR(MAX)");
            builder.Property(e => e.UploadedBy).IsRequired().HasColumnType("NVARCHAR(256)");
            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
}
