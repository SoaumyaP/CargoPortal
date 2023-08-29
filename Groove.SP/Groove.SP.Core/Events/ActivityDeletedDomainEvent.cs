using Groove.SP.Core.Entities;
using MediatR;

namespace Groove.SP.Core.Events
{
    /// <summary>
    /// Event used when an activity is deleted
    /// </summary>
    public class ActivityDeletedDomainEvent : INotification
    {
        public ActivityModel Activity { get; }
        public bool? IsDeletedViaFSApi { get; }
        public ActivityDeletedDomainEvent(ActivityModel activity, bool? isDeletedViaFSApi = false)
        {
            Activity = activity;
            IsDeletedViaFSApi = isDeletedViaFSApi;
        }
    }
}
