using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class ConsignmentModelConfiguration : IEntityTypeConfiguration<ConsignmentModel>
    {
        public void Configure(EntityTypeBuilder<ConsignmentModel> builder)
        {
            builder.Property(e => e.ConsignmentType).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.ShipFrom).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.ShipTo).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.Status).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.ModeOfTransport).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.Movement).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.Package).IsRequired().HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.PackageUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.Unit).IsRequired().HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.UnitUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.Volume).IsRequired().HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.VolumeUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.GrossWeight).IsRequired().HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.GrossWeightUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.NetWeight).IsRequired().HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.NetWeightUOM).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.ExecutionAgentName).HasColumnType("NVARCHAR(512)");
            builder.Property(e => e.ServiceType).HasColumnType("NVARCHAR(128)").HasMaxLength(128);

            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.HasOne(e => e.Shipment)
                .WithMany(e => e.Consignments)
                .HasForeignKey(e => e.ShipmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.HouseBill)
                .WithMany(e => e.Consignments)
                .HasForeignKey(e => e.HouseBillId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.MasterBill)
                .WithMany(e => e.Consignments)
                .HasForeignKey(e => e.MasterBillId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
