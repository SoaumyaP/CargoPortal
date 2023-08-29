using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class SchedulingModelConfiguration : IEntityTypeConfiguration<SchedulingModel>
    {
        public void Configure(EntityTypeBuilder<SchedulingModel> builder)
        {
            builder.Property(e => e.Name).IsRequired().HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.CSPortalReportId).IsRequired();
            builder.Property(e => e.TelerikSchedulingId).IsRequired().HasColumnType("NVARCHAR(128)").HasMaxLength(128);

            builder.HasIndex(e => e.Name).IsUnique();
            builder.HasIndex(e => e.TelerikSchedulingId).IsUnique();
        }
    }
}
