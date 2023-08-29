namespace Groove.SP.Application.Notification.ViewModel
{
    public class PushNotification
    {
        public long NotificationId { get; set; }
        public string MessageKey { get; set; }
        public PushNotificationType Type { get; set; }

        public PushNotification()
        {
            Type = PushNotificationType.New;
        }
    }

    public enum PushNotificationType
    {
        New,
        Read,
        ReadAll
    }
}