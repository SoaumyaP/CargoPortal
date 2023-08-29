using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class SurveyAnswerModelConfiguration : IEntityTypeConfiguration<SurveyAnswerModel>
    {
        public void Configure(EntityTypeBuilder<SurveyAnswerModel> builder)
        {
            builder.Property(e => e.Username).IsRequired(true).HasColumnType("NVARCHAR(256)").HasMaxLength(256);

            builder.HasOne(e => e.Question)
                .WithMany(e => e.Answers)
                .HasForeignKey(e => e.QuestionId) 
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Option)
                .WithMany(e => e.Answers)
                .HasForeignKey(e => e.OptionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
