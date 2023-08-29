using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class ShipmentLoadModelConfiguration : IEntityTypeConfiguration<ShipmentLoadModel>
    {
        public void Configure(EntityTypeBuilder<ShipmentLoadModel> builder)
        {
            builder.Property(e => e.ModeOfTransport).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.LoadingPlace).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.EquipmentType).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            
            builder.Property(e => e.CarrierBookingNo).HasColumnType("VARCHAR(50)").HasMaxLength(50);

            builder.HasIndex(u => new { u.ShipmentId, u.ConsolidationId }).IsUnique();
            builder.HasIndex(u => new { u.ShipmentId, u.ContainerId }).IsUnique();

            builder.HasOne(e => e.Shipment)
                .WithMany(e => e.ShipmentLoads)
                .HasForeignKey(e => e.ShipmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Consignment)
                .WithMany(e => e.ShipmentLoads)
                .HasForeignKey(e => e.ConsignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Container)
                .WithMany(e => e.ShipmentLoads)
                .HasForeignKey(e => e.ContainerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Consolidation)
                .WithMany(e => e.ShipmentLoads)
                .HasForeignKey(e => e.ConsolidationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
