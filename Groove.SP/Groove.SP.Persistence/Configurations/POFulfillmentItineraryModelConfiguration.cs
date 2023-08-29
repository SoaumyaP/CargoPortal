using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class POFulfillmentItineraryModelConfiguration : IEntityTypeConfiguration<POFulfillmentItineraryModel>
    {
        public void Configure(EntityTypeBuilder<POFulfillmentItineraryModel> builder)
        {
            builder.Property(e => e.VesselFlight).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.CarrierName).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.LoadingPort).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.DischargePort).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.HasQueryFilter(e => (int)e.Status == 10); // Active
        }
    }
}
