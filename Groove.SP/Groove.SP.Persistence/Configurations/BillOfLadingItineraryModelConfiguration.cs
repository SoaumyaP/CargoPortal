using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class BillOfLadingItineraryModelConfiguration : IEntityTypeConfiguration<BillOfLadingItineraryModel>
    {
        public void Configure(EntityTypeBuilder<BillOfLadingItineraryModel> builder)
        {
            builder.HasKey(e => new { e.ItineraryId, e.BillOfLadingId });

            builder.HasOne(e => e.Itinerary)
                    .WithMany(e => e.BillOfLadingItineraries)
                    .HasForeignKey(e => e.ItineraryId)
                    .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.BillOfLading)
                    .WithMany(e => e.BillOfLadingItineraries)
                    .HasForeignKey(e => e.BillOfLadingId)
                    .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.MasterBillOfLading)
                    .WithMany(e => e.BillOfLadingItineraries)
                    .HasForeignKey(e => e.MasterBillOfLadingId)
                    .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
