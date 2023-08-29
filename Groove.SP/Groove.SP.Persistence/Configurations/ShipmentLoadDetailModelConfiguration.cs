using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class ShipmentLoadDetailModelConfiguration : IEntityTypeConfiguration<ShipmentLoadDetailModel>
    {
        public void Configure(EntityTypeBuilder<ShipmentLoadDetailModel> builder)
        {
            builder.Property(e => e.Package).IsRequired().HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.PackageUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.Unit).IsRequired().HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.UnitUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.Volume).IsRequired().HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.VolumeUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.GrossWeight).IsRequired().HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.GrossWeightUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.NetWeight).HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.NetWeightUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);

            builder.HasOne(e => e.ShipmentLoad)
                .WithMany(e => e.ShipmentLoadDetails)
                .HasForeignKey(e => e.ShipmentLoadId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
