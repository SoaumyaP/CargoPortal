using Groove.SP.Core.Entities.Cruise;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class CruiseOrderItemModelConfiguration : IEntityTypeConfiguration<CruiseOrderItemModel>
    {
        public void Configure(EntityTypeBuilder<CruiseOrderItemModel> builder)
        {
            builder.Property(x => x.LineEstimatedDeliveryDate).HasColumnType("DATETIME2(7)");
            builder.Property(x => x.FirstReceivedDate).HasColumnType("DATETIME2(7)");
            builder.Property(x => x.MakerReferenceOfItemName2).HasColumnType("NVARCHAR(100)");
            builder.Property(x => x.NetUSUnitPrice).HasColumnType("DECIMAL(18,3)");
            builder.Property(x => x.RequestLineShoreNotes).HasColumnType("NVARCHAR(500)");
            builder.Property(x => x.ShipRequestLineNotes).HasColumnType("NVARCHAR(500)");
            builder.Property(x => x.TotalOrderPrice).HasColumnType("DECIMAL(18,3)");
            builder.Property(x => x.UOM).HasColumnType("INT");
            builder.Property(x => x.CurrencyCode).HasColumnType("NVARCHAR(16)");
            builder.Property(x => x.ItemId).HasColumnType("NVARCHAR(100)");
            builder.Property(x => x.ItemName).HasColumnType("NVARCHAR(256)");
            builder.Property(x => x.NetUnitPrice).HasColumnType("DECIMAL(18,3)");
            builder.Property(x => x.POLine).IsRequired().HasColumnType("INT");

            builder.Property(x => x.Priority).HasColumnType("NVARCHAR(20)");
            builder.Property(x => x.QuotedCostCurrency).HasColumnType("NVARCHAR(16)");
            builder.Property(x => x.ReadyDate).HasColumnType("DATETIME2(7)");
            builder.Property(x => x.REQOnboardDate).HasColumnType("DATETIME2(7)");
            builder.Property(x => x.Origin).HasColumnType("NVARCHAR(500)");
            builder.Property(x => x.ApprovedDate).HasColumnType("DATETIME2(7)");

            // Miscellaneous
            builder.Property(x => x.Sub1).HasColumnType("NVARCHAR(100)");
            builder.Property(x => x.Sub2).HasColumnType("NVARCHAR(100)");
            builder.Property(x => x.Contract).HasColumnType("NVARCHAR(50)");
            builder.Property(x => x.ShipboardLoadingLocation).HasColumnType("NVARCHAR(100)");
            builder.Property(x => x.DeliveryPort).HasColumnType("NVARCHAR(100)");
            builder.Property(x => x.Destination).HasColumnType("NVARCHAR(100)");
            builder.Property(x => x.DestinationCountry).HasColumnType("NVARCHAR(100)");
            builder.Property(x => x.ApprovedBy).HasColumnType("NVARCHAR(100)");
            builder.Property(x => x.Comments).HasColumnType("NVARCHAR(500)");
            builder.Property(x => x.QuotedCost).HasColumnType("DECIMAL(18,3)");
            builder.Property(x => x.DelayCause).HasColumnType("NVARCHAR(500)");
            builder.Property(x => x.DeliveryTicket).HasColumnType("NVARCHAR(100)");
            builder.Property(x => x.BuyerName).HasColumnType("NVARCHAR(100)");

            builder.Property(x => x.OriginalItemId).HasColumnType("BIGINT");
            builder.Property(x => x.OriginalOrganizationId).HasColumnType("BIGINT");

            builder.HasOne(x => x.Order)
                    .WithMany(x => x.Items)
                    .HasForeignKey(x => x.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("CruiseOrderItems", "cruise");

        }
    }
}
