using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class ConsignmentItineraryModelConfiguration : IEntityTypeConfiguration<ConsignmentItineraryModel>
    {
        public void Configure(EntityTypeBuilder<ConsignmentItineraryModel> builder)
        {
            builder.HasKey(e => new { e.ConsignmentId, e.ItineraryId });
            
            builder.HasOne(e => e.Consignment)
                .WithMany(e => e.ConsignmentItineraries)
                .HasForeignKey(e => e.ConsignmentId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.Itinerary)
                    .WithMany(e => e.ConsignmentItineraries)
                    .HasForeignKey(e => e.ItineraryId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Shipment)
                .WithMany(e => e.ConsignmentItineraries)
                .HasForeignKey(e => e.ShipmentId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.MasterBill)
                .WithMany(e => e.ConsignmentItineraries)
                .HasForeignKey(e => e.MasterBillId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
