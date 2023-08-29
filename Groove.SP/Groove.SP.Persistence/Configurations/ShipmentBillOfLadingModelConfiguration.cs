using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class ShipmentBillOfLadingModelConfiguration : IEntityTypeConfiguration<ShipmentBillOfLadingModel>
    {
        public void Configure(EntityTypeBuilder<ShipmentBillOfLadingModel> builder)
        {
            builder.HasKey(e => new { e.ShipmentId, e.BillOfLadingId });

            builder.HasOne(e => e.Shipment)
                    .WithMany(e => e.ShipmentBillOfLadings)
                    .HasForeignKey(e => e.ShipmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.BillOfLading)
                    .WithMany(e => e.ShipmentBillOfLadings)
                    .HasForeignKey(e => e.BillOfLadingId)
                    .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
