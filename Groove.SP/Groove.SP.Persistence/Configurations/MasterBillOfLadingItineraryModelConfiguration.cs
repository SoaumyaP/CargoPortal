using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class MasterBillOfLadingItineraryModelConfiguration : IEntityTypeConfiguration<MasterBillOfLadingItineraryModel>
    {
        public void Configure(EntityTypeBuilder<MasterBillOfLadingItineraryModel> builder)
        {
            builder.HasKey(e => new { e.ItineraryId, e.MasterBillOfLadingId });

            builder.HasOne(e => e.Itinerary)
                    .WithMany(e => e.MasterBillOfLadingItineraries)
                    .HasForeignKey(e => e.ItineraryId)
                    .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.MasterBillOfLading)
                    .WithMany(e => e.MasterBillOfLadingItineraries)
                    .HasForeignKey(e => e.MasterBillOfLadingId)
                    .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
