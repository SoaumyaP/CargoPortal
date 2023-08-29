using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class POFulfillmentCargoReceiveItemModelConfiguration : IEntityTypeConfiguration<POFulfillmentCargoReceiveItemModel>
    {
        public void Configure(EntityTypeBuilder<POFulfillmentCargoReceiveItemModel> builder)
        {
            builder.Property(p => p.Volume).HasColumnType("DECIMAL(18, 4)");

            builder.Property(e => e.DGFlag).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.Reason).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.StyleNo).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.ColourCode).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.SizeCode).HasColumnType("NVARCHAR(256)").HasMaxLength(256);

            builder.HasOne(e => e.POFulfillmentOrder)
                .WithOne(e => e.POFulfillmentCargoReceiveItem)
                .HasForeignKey<POFulfillmentCargoReceiveItemModel>(e => e.POFulfillmentOrderId);
        }
    }
}
