using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.Notification.ViewModel
{
    public class NotificationViewModel : ViewModelBase<NotificationModel>
    {
        public long Id { get; set; }
        public string MessageKey { get; set; }
        public string ReadUrl { get; set; }
        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new System.NotImplementedException();
        }
    }
}
