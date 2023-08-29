using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class POFulfillmentOrderModelConfiguration : IEntityTypeConfiguration<POFulfillmentOrderModel>
    {
        public void Configure(EntityTypeBuilder<POFulfillmentOrderModel> builder)
        {
            builder.Property(p => p.Volume).HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.CustomerPONumber).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.ProductCode).HasColumnType("NVARCHAR(128)").HasMaxLength(128);

            builder.Property(e => e.SeasonCode).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.StyleNo).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.ColourCode).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.Size).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
        }
    }
}
