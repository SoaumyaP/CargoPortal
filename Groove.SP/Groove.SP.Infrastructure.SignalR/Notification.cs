using Groove.SP.Application.Notification.Interfaces;
using Groove.SP.Application.Notification.ViewModel;
using Groove.SP.Infrastructure.SignalR.HubConfigs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Groove.SP.Infrastructure.SignalR
{
    [Authorize]
    public class Notification : INotification
    {
        #region Fields

        private readonly IHubContext<NotificationHub> _hub;

        #endregion Fields

        #region Constructors

        public Notification(IHubContext<NotificationHub> hub)
        {
            _hub = hub;
        }

        #endregion Constructors

        public async Task SendAsync(PushNotification notification, string userId)
        {
            if (notification == null || string.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            await _hub.Clients.User(userId).SendAsync("PushNotification", notification);
        }

        public async Task SendAsync(PushNotification notification, List<string> userIds)
        {
            if (notification == null || userIds.Count == 0)
            {
                return;
            }

            await _hub.Clients.Users(userIds).SendAsync("PushNotification", notification);
        }

        public async Task SendToGroupAsync(PushNotification notification, string groupName)
        {
            if (notification == null || string.IsNullOrWhiteSpace(groupName))
            {
                return;
            }

            await _hub.Clients.Group(groupName).SendAsync("PushNotification", notification);
        }
    }
}