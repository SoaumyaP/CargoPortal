using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class ShareDocumentModelConfiguration : IEntityTypeConfiguration<ShareDocumentModel>
    {
        public void Configure(EntityTypeBuilder<ShareDocumentModel> builder)
        {
            builder.Property(e => e.Id).IsRequired();
            builder.Property(e => e.BlobId).IsRequired();
            builder.Property(e => e.FileName).IsRequired();
            builder.Property(e => e.SharedBy).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
        }
    }
}
