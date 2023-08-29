using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class UserNotificationModelConfiguration : IEntityTypeConfiguration<UserNotificationModel>
    {
        public void Configure(EntityTypeBuilder<UserNotificationModel> builder)
        {
            builder.HasKey(t => new { Id = t.NotificationId, t.Username });
        }
    }
}
