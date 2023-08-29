using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class ContainerModelConfiguration : IEntityTypeConfiguration<ContainerModel>
    {
        public void Configure(EntityTypeBuilder<ContainerModel> builder)
        {
            builder.Property(e => e.ContainerNo).IsRequired().HasColumnType("VARCHAR(50)").HasMaxLength(50);
            builder.Property(e => e.LoadPlanRefNo).IsRequired().HasColumnType("VARCHAR(50)").HasMaxLength(50);
            builder.Property(e => e.ContainerType).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.ShipFrom).IsRequired().HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.ShipTo).IsRequired().HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.SealNo).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.SealNo2).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.Movement).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.TotalGrossWeight).HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.TotalGrossWeightUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.TotalNetWeight).HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.TotalNetWeightUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.TotalPackageUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.TotalVolume).HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.TotalVolumeUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);

            builder.HasIndex(e => new { e.ShipFrom, e.ContainerNo });
            builder.HasIndex(e => new { e.ShipTo, e.ContainerNo });
            builder.HasIndex(e => new { e.ContainerNo , e.LoadPlanRefNo }).IsUnique();
        }
    }
}
