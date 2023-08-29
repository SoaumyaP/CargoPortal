using Groove.SP.Application.Notification.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.Application.Notification.Interfaces
{
    public interface INotification
    {
        Task SendAsync(PushNotification notification, string userId);
        Task SendAsync(PushNotification notification, List<string> userIds);
        Task SendToGroupAsync(PushNotification notification, string groupName);
    }
}