using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class BillOfLadingConsignmentModelConfiguration : IEntityTypeConfiguration<BillOfLadingConsignmentModel>
    {
        public void Configure(EntityTypeBuilder<BillOfLadingConsignmentModel> builder)
        {
            builder.HasKey(e => new { e.BillOfLadingId, e.ConsignmentId });

            builder.HasOne(e => e.Consignment)
                    .WithMany(e => e.BillOfLadingConsignments)
                    .HasForeignKey(e => e.ConsignmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.BillOfLading)
                    .WithMany(e => e.BillOfLadingConsignments)
                    .HasForeignKey(e => e.BillOfLadingId)
                    .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.Shipment)
                   .WithMany(e => e.BillOfLadingConsignments)
                   .HasForeignKey(e => e.ShipmentId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
