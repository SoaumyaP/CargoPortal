using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class MasterDialogModelConfiguration : IEntityTypeConfiguration<MasterDialogModel>
    {
        public void Configure(EntityTypeBuilder<MasterDialogModel> builder)
        {
            builder.Property(e => e.DisplayOn).IsRequired().HasColumnType("NVARCHAR(64)").HasMaxLength(64);
            builder.Property(e => e.FilterCriteria).IsRequired().HasColumnType("NVARCHAR(64)").HasMaxLength(64);
            builder.Property(e => e.FilterValue).IsRequired().HasColumnType("NVARCHAR(250)");
            builder.Property(e => e.Message).IsRequired().HasColumnType("NVARCHAR(1024)").HasMaxLength(1024);
            builder.Property(e => e.Category).IsRequired().HasColumnType("NVARCHAR(250)").HasMaxLength(250);
            builder.Property(e => e.Owner).HasColumnType("NVARCHAR(250)").HasMaxLength(250);
        }
    }
}