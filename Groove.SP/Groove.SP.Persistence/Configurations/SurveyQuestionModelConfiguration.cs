using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class SurveyQuestionModelConfiguration : IEntityTypeConfiguration<SurveyQuestionModel>
    {
        public void Configure(EntityTypeBuilder<SurveyQuestionModel> builder)
        {
            builder.Property(e => e.Content).IsRequired(true);

            builder.Property(e => e.PlaceHolderText).HasColumnType("NVARCHAR(512)").HasMaxLength(512);

            builder.HasOne(e => e.Survey)
                .WithMany(e => e.Questions)
                .HasForeignKey(e => e.SurveyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}