using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class ContainerItineraryModelConfiguration : IEntityTypeConfiguration<ContainerItineraryModel>
    {
        public void Configure(EntityTypeBuilder<ContainerItineraryModel> builder)
        {
            builder.HasKey(e => new { e.ItineraryId, e.ContainerId });

            builder.HasOne(e => e.Itinerary)
                    .WithMany(e => e.ContainerItineraries)
                    .HasForeignKey(e => e.ItineraryId)
                    .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.Container)
                    .WithMany(e => e.ContainerItineraries)
                    .HasForeignKey(e => e.ContainerId)
                    .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
