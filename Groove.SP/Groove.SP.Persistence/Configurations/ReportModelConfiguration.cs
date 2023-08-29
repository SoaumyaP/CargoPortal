using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class ReportModelConfiguration : IEntityTypeConfiguration<ReportModel>
    {
        public void Configure(EntityTypeBuilder<ReportModel> builder)
        {
            builder.Property(e => e.ReportName).IsRequired().HasColumnType("VARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.ReportUrl).IsRequired().HasColumnType("VARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.ReportDescription).IsRequired().HasColumnType("VARCHAR(3000)").HasMaxLength(3000);
            builder.Property(e => e.ReportGroup).IsRequired().HasColumnType("VARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.LastRunTime);
            builder.Property(e => e.StoredProcedureName).HasColumnType("NVARCHAR(128)").HasMaxLength(128);

            builder.Property(e => e.TelerikReportId).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.TelerikCategoryId).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.TelerikCategoryName).HasColumnType("NVARCHAR(512)").HasMaxLength(512);

            builder.HasIndex(e => e.ReportName).IsUnique();
            builder.HasIndex(e => e.ReportUrl).IsUnique();

        }
    }
}
