using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class CargoDetailModelConfiguration : IEntityTypeConfiguration<CargoDetailModel>
    {
        public void Configure(EntityTypeBuilder<CargoDetailModel> builder)
        {
            builder.Property(e => e.ShippingMarks).HasColumnType("NVARCHAR(MAX)");
            builder.Property(e => e.Description).HasColumnType("NVARCHAR(MAX)");
            builder.Property(e => e.Package).IsRequired().HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.PackageUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.Unit).IsRequired().HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.UnitUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.Volume).IsRequired().HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.VolumeUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.GrossWeight).IsRequired().HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.GrossWeightUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.ChargeableWeight).HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.ChargeableWeightUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.VolumetricWeight).HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.VolumetricWeightUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.NetWeight).HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.NetWeightUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.Commodity).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.HSCode).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.ProductNumber).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.CountryOfOrigin).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.OrderType).IsRequired().HasColumnType("INT").HasDefaultValue(OrderType.Freight);

        }
    }
}
