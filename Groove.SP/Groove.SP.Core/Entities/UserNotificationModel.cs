using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Core.Entities
{
    public class UserNotificationModel : Entity
    {
        public long NotificationId { get; set; }
        public string Username { get; set; }
        public bool IsRead { get; set; }
        public NotificationModel Notification { get; set; }
    }
}