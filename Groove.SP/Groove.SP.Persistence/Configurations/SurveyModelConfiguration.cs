using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class SurveyModelConfiguration
    {
        public class ReportModelConfiguration : IEntityTypeConfiguration<SurveyModel>
        {
            public void Configure(EntityTypeBuilder<SurveyModel> builder)
            {
                builder.Property(e => e.Name).IsRequired().HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            }
        }
    }
}
