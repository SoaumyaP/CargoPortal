using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class ConsolidationModelConfiguration : IEntityTypeConfiguration<ConsolidationModel>
    {
        public void Configure(EntityTypeBuilder<ConsolidationModel> builder)
        {
            builder.Property(e => e.ContainerNo).HasColumnType("VARCHAR(50)").HasMaxLength(50);
            builder.Property(e => e.ConsolidationNo).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.SealNo).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.SealNo2).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.EquipmentType).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.OriginCFS).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.ModeOfTransport).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.TotalGrossWeight).HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.TotalGrossWeightUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.TotalNetWeight).HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.TotalNetWeightUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.TotalPackage).HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.TotalPackageUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.TotalVolume).HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.TotalVolumeUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);

            builder.HasOne(e => e.Container)
                .WithOne(e => e.Consolidation)
                .HasForeignKey<ConsolidationModel>(e => e.ContainerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
