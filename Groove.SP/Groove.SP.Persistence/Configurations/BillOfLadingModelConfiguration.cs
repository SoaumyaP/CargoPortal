using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class BillOfLadingModelConfiguration : IEntityTypeConfiguration<BillOfLadingModel>
    {
        public void Configure(EntityTypeBuilder<BillOfLadingModel> builder)
        {
            builder.Property(e => e.BillOfLadingNo).IsRequired().HasColumnType("VARCHAR(50)").HasMaxLength(50);
            builder.Property(e => e.JobNumber).HasColumnType("NVARCHAR(12)").HasMaxLength(12);
            builder.Property(e => e.ModeOfTransport).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            //builder.Property(e => e.ShipmentType).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.ShipFrom).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.ShipTo).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.Movement).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.Incoterm).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.BillOfLadingType).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.MainCarrier).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.MainVessel).HasColumnType("NVARCHAR(512)").HasMaxLength(512);

            builder.Property(e => e.TotalPackage).HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.TotalPackageUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.TotalGrossWeight).HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.TotalGrossWeightUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.TotalNetWeight).HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.TotalNetWeightUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.TotalVolume).HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.TotalVolumeUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);

            builder.HasIndex(e => e.BillOfLadingNo).IsUnique();
        }
    }
}
