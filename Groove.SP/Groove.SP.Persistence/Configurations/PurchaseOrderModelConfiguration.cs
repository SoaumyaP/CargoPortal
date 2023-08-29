using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class PurchaseOrderModelConfiguration : IEntityTypeConfiguration<PurchaseOrderModel>
    {
        public void Configure(EntityTypeBuilder<PurchaseOrderModel> builder)
        {
            builder.Property(e => e.POKey)
                .IsRequired()
                .HasColumnType("VARCHAR(612)");
            builder.Property(e => e.HazardousMaterialsInstruction).HasColumnType("VARCHAR(MAX)");
            builder.Property(e => e.Incoterm).HasColumnType("VARCHAR(3)");
            builder.Property(e => e.ModeOfTransport).HasColumnType("NVARCHAR(MAX)");
            builder.Property(e => e.PONumber).IsRequired().HasColumnType("VARCHAR(512)");
            builder.Property(e => e.PORemark).HasColumnType("NVARCHAR(MAX)");
            builder.Property(e => e.POTerms).HasColumnType("NVARCHAR(512)");
            builder.Property(e => e.CustomerReferences).HasColumnType("NVARCHAR(512)");
            builder.Property(e => e.Department).HasColumnType("NVARCHAR(512)");
            builder.Property(e => e.Season).HasColumnType("NVARCHAR(512)");
            builder.Property(e => e.ShipFrom).HasColumnType("NVARCHAR(512)");
            builder.Property(e => e.ShipTo).HasColumnType("NVARCHAR(512)");
            builder.Property(e => e.SpecialHandlingInstruction).HasColumnType("NVARCHAR(MAX)");
            builder.Property(e => e.PaymentCurrencyCode).HasColumnType("NVARCHAR(16)");
            builder.Property(e => e.CarrierCode).HasColumnType("NVARCHAR(128)");
            builder.Property(e => e.GatewayCode).HasColumnType("NVARCHAR(50)");

            builder.Property(e => e.PaymentTerms).HasColumnType("NVARCHAR(512)");
            builder.Property(e => e.SpecialHandlingInstruction).HasColumnType("NVARCHAR(MAX)");
            builder.Property(e => e.CarrierName).HasColumnType("NVARCHAR(512)");
            builder.Property(e => e.GatewayName).HasColumnType("NVARCHAR(512)");
            builder.Property(e => e.ShipFromName).HasColumnType("NVARCHAR(512)");
            builder.Property(e => e.ShipToName).HasColumnType("NVARCHAR(512)");

            builder.Property(e => e.CreatedBy).IsRequired().HasColumnType("NVARCHAR(128)");
            builder.Property(e => e.Status).IsRequired();
            builder.Property(e => e.Stage).IsRequired();
            builder.Property(e => e.POType).IsRequired().HasDefaultValue(POType.Bulk);

            builder.Property(e => e.Volume).IsRequired(false).HasColumnType("DECIMAL(18,4)");
            builder.Property(e => e.GrossWeight).IsRequired(false).HasColumnType("DECIMAL(18,2)");
            builder.Property(e => e.ContractShipmentDate).IsRequired(false).HasColumnType("DATETIME2(7)");


            builder.HasMany(p => p.AllocatedPOs)
                .WithOne(c => c.BlanketPO)
                .HasForeignKey(c => c.BlanketPOId);

            builder.HasIndex(e => e.POKey);//.IsUnique();
            builder.HasIndex(e => e.PONumber);
            builder.HasIndex(e => new { e.CreatedDate, e.Id });
        }
    }
}
