using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class ViewSettingModelConfiguration : IEntityTypeConfiguration<ViewSettingModel>
    {
        public void Configure(EntityTypeBuilder<ViewSettingModel> builder)
        {
            builder.HasKey(e => e.ViewId);
            builder.Property(e => e.Field).IsRequired().HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.Title).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.ModuleId).IsRequired().HasColumnType("NVARCHAR(256)").HasMaxLength(256);
        }
    }
}
