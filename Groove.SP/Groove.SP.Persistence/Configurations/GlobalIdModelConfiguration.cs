using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class GlobalIdModelConfiguration : IEntityTypeConfiguration<GlobalIdModel>
    {
        public void Configure(EntityTypeBuilder<GlobalIdModel> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.EntityType).IsRequired().HasColumnType("NVARCHAR(64)").HasMaxLength(64);
            builder.Property(e => e.EntityId).IsRequired();
        }
    }
}
