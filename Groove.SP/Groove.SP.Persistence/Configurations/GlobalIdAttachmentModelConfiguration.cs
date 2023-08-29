using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class GlobalIdAttachmentModelConfiguration : IEntityTypeConfiguration<GlobalIdAttachmentModel>
    {
        public void Configure(EntityTypeBuilder<GlobalIdAttachmentModel> builder)
        {
            builder.HasKey(e => new { e.GlobalId, e.AttachemntId });

            builder.HasOne(e => e.ReferenceEntity)
                    .WithMany(e => e.GlobalIdAttachments)
                    .HasForeignKey(e => e.GlobalId)
                    .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.Attachment)
                    .WithMany(e => e.GlobalIdAttachments)
                    .HasForeignKey(e => e.AttachemntId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
