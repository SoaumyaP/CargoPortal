using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class NoteModelConfiguration : IEntityTypeConfiguration<NoteModel>
    {
        public void Configure(EntityTypeBuilder<NoteModel> builder)
        {
            builder.Property(e => e.Category).HasColumnType("NVARCHAR(250)").HasMaxLength(250);
            builder.Property(e => e.NoteText).IsRequired().HasColumnType("NVARCHAR(MAX)");
            builder.Property(e => e.Owner).HasColumnType("NVARCHAR(MAX)");
            builder.Property(e => e.GlobalObjectId).IsRequired().HasColumnType("NVARCHAR(450)").HasMaxLength(450);
        }
    }
}
