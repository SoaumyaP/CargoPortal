using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class GlobalIdActivityModelConfiguration : IEntityTypeConfiguration<GlobalIdActivityModel>
    {
        public void Configure(EntityTypeBuilder<GlobalIdActivityModel> builder)
        {
            builder.HasOne(e => e.ReferenceEntity)
                    .WithMany(e => e.GlobalIdActivities)
                    .HasForeignKey(e => e.GlobalId)
                    .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.Activity)
                    .WithMany(e => e.GlobalIdActivities)
                    .HasForeignKey(e => e.ActivityId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
