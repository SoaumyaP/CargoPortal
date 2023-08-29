using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class SurveyQuestionOptionModelConfiguration : IEntityTypeConfiguration<SurveyQuestionOptionModel>
    {
        public void Configure(EntityTypeBuilder<SurveyQuestionOptionModel> builder)
        {
            builder.Property(e => e.Content).IsRequired(true);

            builder.HasOne(e => e.Question)
                .WithMany(e => e.Options)
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
