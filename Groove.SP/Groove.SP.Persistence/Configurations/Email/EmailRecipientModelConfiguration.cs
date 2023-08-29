using Groove.SP.Core.Entities.Cruise;
using Groove.SP.Core.Entities.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class EmailRecipientModelConfiguration : IEntityTypeConfiguration<EmailRecipientModel>
    {
        public void Configure(EntityTypeBuilder<EmailRecipientModel> builder)
        {
            builder.Property(x => x.TemplateName).IsRequired().HasColumnType("NVARCHAR(256)");
            builder.Property(x => x.To).HasColumnType("NVARCHAR(512)");
            builder.Property(x => x.CC).HasColumnType("NVARCHAR(512)");
            builder.Property(x => x.BCC).HasColumnType("NVARCHAR(512)");



            builder.ToTable("EmailRecipients", "email");

            builder.HasIndex(e => e.TemplateName).IsUnique();

        }
    }
}
