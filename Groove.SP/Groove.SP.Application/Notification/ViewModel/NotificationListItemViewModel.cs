using System;

namespace Groove.SP.Application.Notification.ViewModel
{
    public class NotificationListItemViewModel
    {
        public long Id { get; set; }
        public string MessageKey { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsRead { get; set; }
        public string ReadUrl { get; set; }
    }
}
