using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class ShipmentItemModelConfiguration : IEntityTypeConfiguration<ShipmentItemModel>
    {
        public void Configure(EntityTypeBuilder<ShipmentItemModel> builder)
        {
            builder.Property(e => e.Commodity).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.CustomerPONumber).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.HSCode).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.ProductCode).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.CountryCodeOfOrigin).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.Volume).HasColumnType("DECIMAL(18, 4)");
        }
    }
}