using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class ShipmentModelConfiguration : IEntityTypeConfiguration<ShipmentModel>
    {
        public void Configure(EntityTypeBuilder<ShipmentModel> builder)
        {
            builder.Property(e => e.ShipmentNo).IsRequired().HasColumnType("VARCHAR(50)").HasMaxLength(50);
            builder.Property(e => e.BookingNo).HasColumnType("VARCHAR(50)").HasMaxLength(50);
            builder.Property(e => e.CustomerReferenceNo).HasColumnType("VARCHAR(3000)").HasMaxLength(3000);
            builder.Property(e => e.AgentReferenceNo).HasColumnType("VARCHAR(3000)").HasMaxLength(3000);
            builder.Property(e => e.ShipperReferenceNo).HasColumnType("VARCHAR(3000)").HasMaxLength(3000);
            builder.Property(e => e.ModeOfTransport).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.ShipmentType).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.ShipFrom).IsRequired().HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.ShipTo).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.Movement).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.TotalPackage).HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.TotalPackageUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.TotalUnit).HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.TotalUnitUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.TotalGrossWeight).HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.TotalGrossWeightUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.TotalNetWeight).HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.TotalNetWeightUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.TotalVolume).HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.TotalVolumeUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.ServiceType).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.Incoterm).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.Status).IsRequired().HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.OrderType).IsRequired().HasColumnType("INT").HasDefaultValue(OrderType.Freight);
            builder.Property(e => e.CommercialInvoiceNo).HasColumnType("VARCHAR(50)").HasMaxLength(50);

            // Navigation property via null-able foreign key
            builder.HasOne(e => e.ContractMaster).WithMany().HasForeignKey(x => x.CarrierContractNo).HasPrincipalKey(x=>x.CarrierContractNo).IsRequired(false);

            builder.HasIndex(e => new { e.ShipFromETDDate });
            builder.HasIndex(e => new { e.CargoReadyDate, e.Id });
            builder.HasIndex(e => e.ShipmentNo).IsUnique();
        }
    }
}
