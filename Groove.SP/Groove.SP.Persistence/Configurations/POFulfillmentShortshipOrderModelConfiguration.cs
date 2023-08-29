using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class POFulfillmentShortshipOrderModelConfiguration : IEntityTypeConfiguration<POFulfillmentShortshipOrderModel>
    {
        public void Configure(EntityTypeBuilder<POFulfillmentShortshipOrderModel> builder)
        {
            builder.Property(e => e.POFulfillmentNumber).HasColumnType("NVARCHAR(20)").HasMaxLength(20).IsRequired();
            builder.Property(p => p.Volume).HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.CustomerPONumber).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.ProductCode).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
        }
    }
}