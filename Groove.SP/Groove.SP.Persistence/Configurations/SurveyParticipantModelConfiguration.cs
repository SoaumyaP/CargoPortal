using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class SurveyParticipantModelConfiguration : IEntityTypeConfiguration<SurveyParticipantModel>
    {
        public void Configure(EntityTypeBuilder<SurveyParticipantModel> builder)
        {
            builder.HasKey(t => new { Id = t.SurveyId, t.Username });

            builder.HasOne(e => e.Survey)
                .WithMany(e => e.Participants)
                .HasForeignKey(e => e.SurveyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
