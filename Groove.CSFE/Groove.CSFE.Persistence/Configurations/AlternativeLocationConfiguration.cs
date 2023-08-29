using Groove.CSFE.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.CSFE.Persistence.Configurations
{
    public class AlternativeLocationConfiguration : IEntityTypeConfiguration<AlternativeLocationModel>
    {
        public void Configure(EntityTypeBuilder<AlternativeLocationModel> builder)
        {
            builder.Property(e => e.Name).HasColumnType("nvarchar(128)");

            builder.HasOne(rt => rt.Location)
                .WithMany(cc => cc.AlternativeLocations)
                .HasForeignKey(rt => rt.LocationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}