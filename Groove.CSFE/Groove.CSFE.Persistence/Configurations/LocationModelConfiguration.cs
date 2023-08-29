using Groove.CSFE.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.CSFE.Persistence.Configurations
{
    public class LocationModelConfiguration : IEntityTypeConfiguration<LocationModel>
    {
        public void Configure(EntityTypeBuilder<LocationModel> builder)
        {
            builder.Property(e => e.Name).HasColumnType("nvarchar(128)");

            builder.Property(e => e.LocationDescription).IsRequired().HasColumnType("nvarchar(128)");

            builder.Property(e => e.EdiSonPortCode).HasColumnType("nvarchar(128)");

            builder.HasOne(rt => rt.Country)
                .WithMany(cc => cc.Locations)
                .HasForeignKey(rt => rt.CountryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
