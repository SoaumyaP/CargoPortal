using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class POFulfillmentCargoDetailModelConfiguration : IEntityTypeConfiguration<POFulfillmentCargoDetailModel>

    {
        public void Configure(EntityTypeBuilder<POFulfillmentCargoDetailModel> builder)
        {
            builder.Property(p => p.GrossWeight).HasColumnType("DECIMAL(18, 4)");
            builder.Property(p => p.Height).HasColumnType("DECIMAL(18, 4)");
            builder.Property(p => p.Length).HasColumnType("DECIMAL(18, 4)");
            builder.Property(p => p.NetWeight).HasColumnType("DECIMAL(18, 4)");
            builder.Property(p => p.Volume).HasColumnType("DECIMAL(18, 4)");
            builder.Property(p => p.Width).HasColumnType("DECIMAL(18, 4)");
        }
    }
}