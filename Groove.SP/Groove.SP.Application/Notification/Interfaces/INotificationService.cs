using Groove.SP.Application.Common;
using Groove.SP.Application.Notification.ViewModel;
using Groove.SP.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.Application.Notification.Interfaces
{
    public interface INotificationService : IServiceBase<NotificationModel, NotificationViewModel>
    {
        public Task<ListPagingViewModel<NotificationListItemViewModel>> GetByUserNameAsync(string userName, int skip = 0, int take = 5);

        public Task<int> GetUnreadTotalAsync(string userName);

        public Task ReadAsync(long id, string userName);

        public Task ReadAllAsync(string userName);

        /// <summary>
        /// Create and send notification to all users in the organization.
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="notification"></param>
        /// <returns></returns>
        public Task PushNotificationSilentAsync(long orgId, NotificationViewModel notification);

        /// <summary>
        /// Create and send notification to users by userId.
        /// </summary>
        /// <param name="userIds"></param>
        /// <param name="emails"></param>
        /// <param name="notification"></param>
        /// <returns></returns>
        public Task PushNotificationSilentAsync(List<string> userIds, List<string> emails, NotificationViewModel notification);

        /// <summary>
        /// Create and send notification to users by userName.
        /// </summary>
        /// <param name="userNames"></param>
        /// <param name="notification"></param>
        /// <returns></returns>
        Task PushNotificationSilentAsync(List<string> userNames, NotificationViewModel notification);
    }
}