using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class POLineItemModelConfiguration : IEntityTypeConfiguration<POLineItemModel>
    {
        public void Configure(EntityTypeBuilder<POLineItemModel> builder)
        {
            builder.Property(e => e.POLineKey)
                .IsRequired()
                .HasColumnType("VARCHAR(750)");
            builder.Property(e => e.Commodity).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.HSCode).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.ProductCode).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.SupplierProductCode).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.ReferenceNumber1).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.ReferenceNumber2).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.CurrencyCode).HasColumnType("NVARCHAR(16)").HasMaxLength(16);
            builder.Property(e => e.CountryCodeOfOrigin).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.UnitPrice).HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.SeasonCode).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.StyleNo).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.ColourCode).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.Size).HasColumnType("NVARCHAR(256)").HasMaxLength(256);

            builder.Property(e => e.Volume).IsRequired(false).HasColumnType("DECIMAL(18,4)");
            builder.Property(e => e.GrossWeight).IsRequired(false).HasColumnType("DECIMAL(18,2)");

            builder.Property(e => e.InboundDelivery).IsRequired(false).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.POItemReference).IsRequired(false).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.ShipmentNo).IsRequired(false).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.Plant).IsRequired(false).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.StorageLocation).IsRequired(false).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.MatGrpDe).IsRequired(false).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.MaterialType).IsRequired(false).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.Sku).IsRequired(false).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.GridValue).IsRequired(false).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.StockCategory).IsRequired(false).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.HeaderText).IsRequired(false).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.Length).IsRequired(false).HasColumnType("DECIMAL(18,4)");
            builder.Property(e => e.Width).IsRequired(false).HasColumnType("DECIMAL(18,4)");
            builder.Property(e => e.Height).IsRequired(false).HasColumnType("DECIMAL(18,4)");
            builder.Property(e => e.NetWeight).IsRequired(false).HasColumnType("DECIMAL(18,4)");
            builder.Property(e => e.FactoryName).IsRequired(false).HasColumnType("NVARCHAR(256)").HasMaxLength(256);


            builder.HasOne(e => e.PurchaseOrder)
                    .WithMany(e => e.LineItems)
                    .HasForeignKey(e => e.PurchaseOrderId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
