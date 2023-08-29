using Groove.CSFE.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.CSFE.Persistence.Configurations
{
    public class EmailNotificationModelConfiguration: IEntityTypeConfiguration<EmailNotificationModel>
    {
        public void Configure(EntityTypeBuilder<EmailNotificationModel> builder)
        {
            builder.Property(e => e.Email).HasColumnType("NVARCHAR(255)").IsRequired();

            builder.HasOne(e => e.Customer)
              .WithMany()
              .HasForeignKey(e => e.CustomerId);
        }
    }
}