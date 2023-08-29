using System.Collections.Generic;

namespace Groove.SP.Core.Entities
{
    public class NotificationModel : Entity
    {
        public long Id { get; set; }
        public string MessageKey { get; set; }
        public string ReadUrl { get; set; }
        public virtual ICollection<UserNotificationModel> UserNotifications { get; set; }
    }
}