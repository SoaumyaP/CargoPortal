using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class BillOfLadingShipmentLoadModelConfiguration : IEntityTypeConfiguration<BillOfLadingShipmentLoadModel>
    {
        public void Configure(EntityTypeBuilder<BillOfLadingShipmentLoadModel> builder)
        {
            builder.HasKey(e => new { e.BillOfLadingId, e.ShipmentLoadId });

            builder.HasOne(e => e.BillOfLading)
                .WithMany(e => e.BillOfLadingShipmentLoads)
                .HasForeignKey(e => e.BillOfLadingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.ShipmentLoad)
                .WithMany(e => e.BillOfLadingShipmentLoads)
                .HasForeignKey(e => e.ShipmentLoadId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(e => e.Container)
                .WithMany(e => e.BillOfLadingShipmentLoads)
                .HasForeignKey(e => e.ContainerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Consolidation)
                .WithMany(e => e.BillOfLadingShipmentLoads)
                .HasForeignKey(e => e.ConsolidationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.MasterBillOfLading)
                .WithMany(e => e.BillOfLadingShipmentLoads)
                .HasForeignKey(e => e.MasterBillOfLadingId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
